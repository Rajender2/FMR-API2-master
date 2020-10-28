using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services
{
    public static class Extensions
    {

        public static void TryUpdateManyToMany<T, TKey>(this DbContext db, IEnumerable<T> currentItems, IEnumerable<T> newItems, Func<T, TKey> getKey, bool UpdateAll) where T : class
        {
            if (UpdateAll)
            {
                db.Set<T>().RemoveRange(currentItems);
                db.Set<T>().AddRange(newItems);
            }
            else
            {
                db.Set<T>().RemoveRange(currentItems.Except(newItems, getKey));
                db.Set<T>().AddRange(newItems.Except(currentItems, getKey));
            }
        }

        public static IEnumerable<T> Except<T, TKey>(this IEnumerable<T> items, IEnumerable<T> other, Func<T, TKey> getKeyFunc)
        {
            return items
                .GroupJoin(other, getKeyFunc, getKeyFunc, (item, tempItems) => new { item, tempItems })
                .SelectMany(t => t.tempItems.DefaultIfEmpty(), (t, temp) => new { t, temp })
                .Where(t => ReferenceEquals(null, t.temp) || t.temp.Equals(default(T)))
                .Select(t => t.t.item);
        }

        public static void AddOrUpdate(this DbContext ctx, object entity)
        {
                var entry = ctx.Entry(entity);
                switch (entry.State)
                {
                    case EntityState.Detached:
                        ctx.Add(entity);
                        break;
                    case EntityState.Modified:
                        ctx.Update(entity);
                        break;
                    case EntityState.Added:
                        ctx.Add(entity);
                        break;
                    case EntityState.Unchanged:
                        //item already in db no need to do anything  
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
       }
            /**
            var customer = context.Customers.SingleOrDefault(c => c.Id == customerId) 
            ?? new Customer();  
            //update some properties  
            context.AddOrUpdate(customer);  
            context.SaveChanges();  **/
        // https://www.michaelgmccarthy.com/2016/08/24/entity-framework-addorupdate-is-a-destructive-operation/

    }
}
