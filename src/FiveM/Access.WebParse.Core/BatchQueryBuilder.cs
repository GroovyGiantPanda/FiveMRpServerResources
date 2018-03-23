using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Access.WebParse.Core
{
	class BatchQueryBuilder : IDisposable
	{
		private bool debugPrintEnabled = true;
		public bool WaitForPartialCommits { get; set; }

		public StringBuilder sbBatchSql { get; set; }
		public string ConnString { get; set; }
		public string Query { get; set; }
		public int CharacterThreshold { get; set; }

		public BatchQueryBuilder(string connString, int characterThreshold = 64000, bool waitForPartialCommits = false)
		{
			sbBatchSql = new StringBuilder();
			this.ConnString = connString;
			this.CharacterThreshold = characterThreshold;
			this.WaitForPartialCommits = waitForPartialCommits;
		}

		public void AppendSqlCommand(MySqlCommand sqlCommand)
		{
			string query = sqlCommand.CommandText;

			foreach (MySqlParameter sqlParameter in sqlCommand.Parameters)
			{
				query = query.Replace($"@{sqlParameter.ParameterName}", ParameterValueForSql(sqlParameter));
			}

			if (debugPrintEnabled) Console.WriteLine($"Adding query: {query}");

			sbBatchSql.Append(query);
			if (sbBatchSql.Length > CharacterThreshold && !WaitForPartialCommits)
				this.Flush();
		}

		public void PartialCommit()
		{
			this.Flush();
		}

		public void AppendSqlCommand(string sqlCommand)
		{
			AppendSqlCommand(new MySqlCommand(sqlCommand));
		}

		internal static string ParameterValueForSql(MySqlParameter sqlParameter)
		{
			string s;

			if (sqlParameter.Value == null) return "null";

			switch (sqlParameter.DbType)
			{
				default:
					s = sqlParameter.Value.ToString().Replace("'", "''");
					break;
			}

			return $"'{s}'";
		}

		public void Flush(bool isDisposal = false)
		{
			if (sbBatchSql.Length == 0) return;
			using (MySqlConnection mysqlConnection = new MySqlConnection(ConnString))
			{
				mysqlConnection.Open();
				MySqlCommand mySqlCommand = new MySqlCommand(sbBatchSql.ToString(), mysqlConnection);
				MySqlTransaction mySqlTransaction = mysqlConnection.BeginTransaction();
				mySqlCommand.Transaction = mySqlTransaction;
				try
				{
					if (debugPrintEnabled) Console.WriteLine($"Executing batch query (Length: {sbBatchSql.Length})");
					mySqlCommand.ExecuteNonQuery();
					mySqlTransaction.Commit();
					if (debugPrintEnabled) Console.WriteLine($"Query committed");
				}
				catch (Exception ex)
				{
					mySqlTransaction.Rollback();
					if (debugPrintEnabled) Console.WriteLine($"Query rolled back; error {ex}");
				}
			}

			sbBatchSql.Clear();
		}

		public void Dispose()
		{
			this.Flush(true);
		}
	}
}
