// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

		[Category("Restriction Operators")]
		[Title("Where - Task 1")]
		[Description("Выдайте список всех клиентов, чей суммарный оборот (сумма всех заказов) превосходит некоторую величину X")]
		public void Linq1()
		{
		    var sum = 10000;

		    var customers = dataSource.Customers
		        .Where(c => c.Orders.Sum(x => x.Total) > sum);

		    foreach (var c in customers)
		    {
		        ObjectDumper.Write(c, 2);
		    }
        }

		[Category("Restriction Operators")]
		[Title("Where - Task 2")]
		[Description("Для каждого клиента составьте список поставщиков, находящихся в той же стране и том же городе.")]

		public void Linq2()
		{
		    var result1 = dataSource.Customers
		        .Join(dataSource.Suppliers,
		            c => new { c.City, c.Country },
		            s => new { s.City, s.Country },
		            (c, s) => new { Customer = c, Suppliers = s });

            ObjectDumper.Write("Without grouping\n");
		    foreach (var c in result1)
		    {
		        ObjectDumper.Write($"Customer: {c.Customer.CompanyName} \n" +
		                           $"   Supplier: {c.Suppliers.SupplierName}");
		    }

		    var result2 = dataSource.Customers
		        .GroupJoin(dataSource.Suppliers,
		        c => new { c.City, c.Country },
		        s => new { s.City, s.Country },
		        (c, s) => new { Customer = c, Suppliers = s });

		    ObjectDumper.Write("With  grouping:\n");
		    foreach (var c in result2)
		    {
		        ObjectDumper.Write($"Customer: {c.Customer.CompanyName} \n" +
		                           $"   Suppliers: {string.Join(", ", c.Suppliers.Select(s => s.SupplierName))}");
		    }
        }

	    [Category("Restriction Operators")]
	    [Title("Where - Task 3")]
	    [Description("Найдите всех клиентов, у которых были заказы, превосходящие по сумме величину X")]
	    public void Linq3()
	    {
	        var x = 1000;

	        var customers = dataSource.Customers
	            .Where(c => c.Orders.Any(o => o.Total > x));

	        foreach (var c in customers)
	        {
	            ObjectDumper.Write(c.CompanyName);
	        }
	    }

	    [Category("Restriction Operators")]
	    [Title("Where - Task 4")]
	    [Description("Выдайте список клиентов с указанием, начиная с какого месяца какого года они стали клиентами (принять за таковые месяц и год самого первого заказа)")]
	    public void Linq4()
	    {
	        var customers = dataSource.Customers
	            .Select(c =>
	            {
	                var startDate = c.Orders?.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate;
	                return new {c.CompanyName, date = $"{startDate?.Month}.{startDate?.Year}"};
	            });

	        foreach (var c in customers)
	        {
	            ObjectDumper.Write(c);
	        }
	    }

	    [Category("Restriction Operators")]
	    [Title("Where - Task 5")]
	    [Description("Сделайте предыдущее задание, но выдайте список отсортированным по году, месяцу, оборотам клиента (от максимального к минимальному) и имени клиента")]
	    public void Linq5()
	    {
	        var customers = dataSource.Customers
	            .Select(c =>
	            {
	                var startDate = c.Orders?.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate;
	                return new { c.CompanyName, startDate?.Month, startDate?.Year, TotalSum = c.Orders?.Sum(o => o.Total) };
	            });

	        var result = customers.OrderBy(c => c.Year)
	            .ThenBy(c => c.Month)
	            .ThenByDescending(c => c.TotalSum)
	            .ThenBy(c => c.CompanyName);

	        foreach (var r in result)
	        {
	            ObjectDumper.Write(r);
	        }
	    }

	    [Category("Restriction Operators")]
	    [Title("Where - Task 6")]
	    [Description("Укажите всех клиентов, у которых указан нецифровой почтовый код или не заполнен регион" +
	                 " или в телефоне не указан код оператора (для простоты считаем, что это равнозначно «нет круглых скобочек в начале»)")]
	    public void Linq6()
	    {
	        var customers = dataSource.Customers.Where(c =>
                !int.TryParse(c.PostalCode, out var n)
                || string.IsNullOrEmpty(c.Region)
                || !c.Phone.StartsWith("(")
            );

	        foreach (var c in customers)
	        {
	            ObjectDumper.Write(c);
	        }
	    }

	    [Category("Restriction Operators")]
	    [Title("Where - Task 7")]
	    [Description("Сгруппируйте все продукты по категориям, внутри – по наличию на складе, внутри последней группы отсортируйте по стоимости")]
	    public void Linq7()
	    {
            var products = dataSource.Products
                .GroupBy(p => p.Category)
                .Select(p => new
                {
                    Category = p.Key,
                    inStock = p
                        .GroupBy(product => product.UnitsInStock)
                        .Select(product => new
                        {
                            UnitsInStock = product.Key,
                            Products = product.OrderBy(pro => pro.UnitPrice)
                        })
                });

	        foreach (var p in products)
	        {
	            ObjectDumper.Write(p, 2);
	        }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 8")]
        [Description("Сгруппируйте товары по группам «дешевые», «средняя цена», «дорогие». Границы каждой группы задайте сами")]
        public void Linq8()
        {
            const int cheaperPrice = 50;
            const int moreExpensivePrice = 100;

            var products = dataSource.Products
                .GroupBy(p => p.UnitPrice < cheaperPrice ? "Cheap" : p.UnitPrice < moreExpensivePrice ? "Average" : "Expensive");

            foreach (var group in products)
            {
                ObjectDumper.Write($"{group.Key}:");
                foreach (var product in group)
                {
                    ObjectDumper.Write($"Product: {product.ProductName} Price: {product.UnitPrice}\n");
                }
            }
        }

        [Category("Restriction Operators")]
	    [Title("Where - Task 9")]
	    [Description("Рассчитайте среднюю прибыльность каждого города (среднюю сумму заказа по всем клиентам из данного города)" +
	                 " и среднюю интенсивность (среднее количество заказов, приходящееся на клиента из каждого города)")]
	    public void Linq9()
	    {
	        var result = dataSource.Customers
	            .GroupBy(c => c.City)
	            .Select(c => new
	            {
	                City = c.Key,
	                AverageIncome = c.Average(p => p.Orders.Sum(o => o.Total)),
                    Intensity = c.Average(p => p.Orders.Length)
	            });

            foreach (var r in result)
            {
                ObjectDumper.Write(r);
            }
        }

        [Category("Restriction Operators")]
	    [Title("Where - Task 10")]
	    [Description("Сделайте среднегодовую статистику активности клиентов по месяцам (без учета года)," +
	                 " статистику по годам, по годам и месяцам (т.е. когда один месяц в разные годы имеет своё значение).")]
	    public void Linq10()
	    {
	        var result = dataSource.Customers
	            .Select(c => new
	            {
	                c.CustomerID,
	                MonthsStat = c.Orders.GroupBy(o => o.OrderDate.Month)
	                    .Select(g => new { Month = g.Key, OrdersCount = g.Count() }),
	                YearsStat = c.Orders.GroupBy(o => o.OrderDate.Year)
	                    .Select(g => new { Year = g.Key, OrdersCount = g.Count() }),
	                YearMonthStat = c.Orders
	                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
	                    .Select(g => new { g.Key.Year, g.Key.Month, OrdersCount = g.Count() })
	            });

	        foreach (var r in result)
	        {
	            ObjectDumper.Write(r, 4);
	        }
        }
    }
}
