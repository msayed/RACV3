using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RACAPI.Models {
  public class Database {
    private readonly string _connectionString;

    public Database(string connectionString) {
      _connectionString = connectionString;
    }

    public object ExecuteScaler(string sql, List<SqlParameter> paramIdent, bool isStoredProcdure) {
      return ExecuteScaler(sql, paramIdent, isStoredProcdure, new SqlConnection(_connectionString));
    }

    public object ExecuteScaler(string sql, List<SqlParameter> paramIdent, bool isStoredProcdure, SqlConnection sqlConnection) {
      SqlCommand cmd = sqlConnection.CreateCommand();
      cmd.CommandText = sql;
      if (isStoredProcdure) {
        cmd.CommandType = CommandType.StoredProcedure;
      }


      if (paramIdent != null) {
        for (int i = 0; i <= paramIdent.Count - 1; i++) {
          cmd.Parameters.Add(paramIdent[i]);
        }
      }

      cmd.Connection.Open();
      cmd.CommandTimeout = cmd.CommandTimeout * 7;
      object val = cmd.ExecuteScalar();
      cmd.Connection.Close();
      return val;
    }

    public int ExecuteNonQuery(string sql, List<SqlParameter> paramIdent, bool isStoredProcdure) {
      return ExecuteNonQuery(sql, paramIdent, isStoredProcdure, new SqlConnection(_connectionString));
    }

    public int ExecuteNonQuery(string sql, List<SqlParameter> paramIdent, bool isStoredProcdure, SqlConnection sqlConnection) {
      using (SqlCommand cmd = sqlConnection.CreateCommand()) {
        //SqlCommand cmd = sqlConnection.CreateCommand();
        cmd.CommandText = sql;
        if (isStoredProcdure) {
          cmd.CommandType = CommandType.StoredProcedure;
        }

        if (paramIdent != null) {
          for (int i = 0; i <= paramIdent.Count - 1; i++) {
            cmd.Parameters.Add(paramIdent[i]);
          }
        }

        cmd.Connection.Open();
        cmd.CommandTimeout = cmd.CommandTimeout * 7;
        int val = cmd.ExecuteNonQuery();
        cmd.Connection.Close();
        return val;
      }
    }

    public DataTable ExecuteQuery(string sql, List<SqlParameter> paramIdent, bool isStoredProcdure) {
      return ExecuteQuery(sql, paramIdent, isStoredProcdure, new SqlConnection(_connectionString));
    }

    public Task<DataTable> ExecuteQueryAsync(string sql, List<SqlParameter> paramIdent, bool isStoredProcdure) {
      return ExecuteQueryAsync(sql, paramIdent, isStoredProcdure, new SqlConnection(_connectionString));
    }

    public async Task<DataTable> ExecuteQueryAsync(string sql, List<SqlParameter> paramIdent, bool isStoredProcdure, SqlConnection sqlConnection) {
      using (SqlCommand cmd = new SqlCommand()) {
        cmd.CommandText = sql;
        cmd.CommandTimeout = 20000;
        cmd.Connection = sqlConnection;
        DataTable result = new DataTable();
        if (isStoredProcdure) {
          cmd.CommandType = CommandType.StoredProcedure;
        }

        if (paramIdent != null) {
          for (int i = 0; i <= paramIdent.Count - 1; i++) {
            cmd.Parameters.Add(paramIdent[i]);
          }
        }

        await cmd.Connection.OpenAsync();
        result.Load(await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection));
        return result;
      }
    }

    public DataTable ExecuteQuery(string sql, List<SqlParameter> paramIdent, bool isStoredProcdure, SqlConnection sqlConnection) {
      using (SqlCommand cmd = new SqlCommand()) {
        cmd.CommandText = sql;
        cmd.CommandTimeout = 20000;
        cmd.Connection = sqlConnection;
        DataTable ReturnedDatatable = new DataTable();
        if (isStoredProcdure) {
          cmd.CommandType = CommandType.StoredProcedure;
        }

        if (paramIdent != null) {
          for (int i = 0; i <= paramIdent.Count - 1; i++) {
            cmd.Parameters.Add(paramIdent[i]);
          }
        }

        cmd.Connection.Open();
        ReturnedDatatable.Load(cmd.ExecuteReader(CommandBehavior.CloseConnection));
        // sqlConnection.Dispose();
        return ReturnedDatatable;
      }
    }


    public SqlDataReader ExecuteQueryReturnedSqlReader(string sql, List<SqlParameter> paramIdent, bool isStoredProcdure) {
      return ExecuteQueryReturnedSqlReader(sql, paramIdent, isStoredProcdure, new SqlConnection(_connectionString));
    }

    public SqlDataReader ExecuteQueryReturnedSqlReader(string sql, List<SqlParameter> paramIdent, bool isStoredProcdure, SqlConnection sqlConnection) {
      SqlCommand cmd = sqlConnection.CreateCommand();
      cmd.CommandText = sql;

      DataTable ReturnedDatatable = new DataTable();
      SqlDataReader sqlDataReader;
      if (isStoredProcdure) {
        cmd.CommandType = CommandType.StoredProcedure;
      }

      if (paramIdent != null) {
        for (int i = 0; i <= paramIdent.Count - 1; i++) {
          cmd.Parameters.Add(paramIdent[i]);
        }
      }

      cmd.Connection.Open();
      ReturnedDatatable.Load(cmd.ExecuteReader(CommandBehavior.CloseConnection));
      sqlDataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
      return sqlDataReader;
    }
  }
}