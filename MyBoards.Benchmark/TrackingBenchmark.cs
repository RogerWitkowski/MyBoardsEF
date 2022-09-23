using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using MyBoards.Data;

namespace MyBoards.Benchmark
{
    [MemoryDiagnoser]
    public class TrackingBenchmark
    {
        [Benchmark]
        public int WithTracking()
        {
            var optionBuilder = new DbContextOptionsBuilder<MyBoardsDbContext>()
                .UseSqlServer("Server=localhost;Database=MyBoardEF_DB;Trusted_Connection=True;MultipleActiveResultSets=True");

            var _dbContext = new MyBoardsDbContext(optionBuilder.Options);

            var comments = _dbContext.Comments
                .ToList();
            return comments.Count;
        }

        [Benchmark]
        public int WithNoTracking()
        {
            var optionBuilder = new DbContextOptionsBuilder<MyBoardsDbContext>()
                .UseSqlServer("Server=localhost;Database=MyBoardEF_DB;Trusted_Connection=True;MultipleActiveResultSets=True");

            var _dbContext = new MyBoardsDbContext(optionBuilder.Options);

            var comments = _dbContext.Comments
                .AsNoTracking()
                .ToList();

            return comments.Count;
        }
    }
}