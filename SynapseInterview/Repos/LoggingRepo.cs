using SynapseInterview.Code.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;

namespace SynapseInterview.Code.Repos
{
    public class LoggingRepo
    {
        private readonly string _connectionString;
        public LoggingRepo(string constr)
        {
            _connectionString = constr;
        }

        public async Task InsertLog(Log log)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.InsertAsync(log);
        }
    }

}
