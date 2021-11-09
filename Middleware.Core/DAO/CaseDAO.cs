using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Middleware.Core.DTO;
using Middleware.Core.Model;

namespace Middleware.Core.DAO
{
    public class CaseDAO : BaseDAO, ICaseDAO
    {
        public CaseDAO(IDbConnection connection) : base(connection)
        {
        }

        public async Task<long> Add(Case item)
        {
            var query = @"INSERT INTO Cases
                           (
                            [state],
                            requestreference,
                            datecreated,
                            accountid,
                            accounttype,
                            customer_id,
                            DateOfBirth)
                          VALUES
                            (
                             @State, 
                             @RequestReference,
                             @DateCreated,
                             @AccountId,
                             @AccountType,
                             @Customer_id,
                             @DateOfBirth
                            );
                         SELECT SCOPE_IDENTITY()";
            return await _connection.ExecuteScalarAsync<long>(query, item, UnitOfWorkSession?.GetTransaction());
        }

        public async Task<Case> Find(string reference)
        {
            var item = await _connection.QuerySingleOrDefaultAsync<Case>(@"SELECT * FROM Cases WHERE requestreference=@reference",
                                                                new { reference });
            if (item == null)
            {
                return null;
            }
            item.Documents = await _connection.QueryAsync<Document>(@"SELECT * FROM Documents WHERE case_id = @Id", new { item.Id });
            return item;
        }

        public async Task<IEnumerable<Case>> FindByState(CaseState caseState)
        {
            var sql = "Select c.*, i.* from Cases c, Customers i where c.Customer_Id = i.id and c.State = @State order by c.datecreated";
            return (await _connection.QueryAsync<Case, Customer, Case>(sql,
                (d, c) =>
                {
                    d.Customer = c;
                    return d;
                }, new Case { State = caseState },
                splitOn: "ID"
                )).AsList();
        }
        public async Task<Case> Find(long customerId, AccountType type)
        {
            var item = await _connection.QuerySingleOrDefaultAsync<Case>(@"SELECT * FROM Cases WHERE customer_id=@customerId
                             AND accounttype=@type", new { customerId, type });
            if (item == null)
            {
                return null;
            }
            item.Documents = await _connection.QueryAsync<Document>(@"SELECT * FROM Documents WHERE case_id = @Id", new { item.Id });
            return item;
        }

        //public IEnumerable<Case> FindByState(CaseState caseState)
        //{
        //    // IEnumerable<Case> result = new List<Case>();
        //    var sql = "Select c.* from Cases c where c.state = @state ";
        //    return _connection.Query<Case>(sql, new Case { State = caseState });
        //}
        public async Task Update(Case item)
        {
            var query = @"UPDATE Cases
                          SET  [requestreference] = @requestreference,
                               serverreference = @serverreference,
                               [state] = @State,
                               [comments] = @comments,
                               [accountid] = @accountid,
                               [accounttype] = @accounttype
                          WHERE id = @Id";
            await _connection.ExecuteAsync(query, item, UnitOfWorkSession?.GetTransaction());
        }
    }
}
