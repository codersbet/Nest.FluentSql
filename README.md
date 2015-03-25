# Nest.FluentSql

Nest.FluentSql is a lightweight library built on top of the popular NEST-Elasticsearch Client. It exposes an extension method to help write a SQL like fluent queries when quering elastic search.

Usage

var client = GetElasticClient();

Querying with the WHERE clause

var result = client.Select<Property>() .Where(x => x.PropertyTypeId).Equals((int) PropertyTypes.Office) .Execute();

Querying with the AND Operator

var result = client.Select<Property>() .Where(x => x.PropertyTypeId).Equals((int)PropertyTypes.Office) .And(x => x.Year).Equals(1985) .Execute();

Querying with the OR operator

var result = client.Select<Property>() .Where(x => x.PropertyTypeId).Equals((int)PropertyTypes.Office) .Or(x => x.Year).Equals(1985) .Execute();

Querying with OR, AND and BETWEEN operators

var result = client.Select<Property>() .Where(x => x.PropertyTypeId).Equals((int)PropertyTypes.Office) .Or(x => x.Year).Equals(1985) .And(x => x.BuiltDate).Between(DateTime.Parse("2014-02-04"), DateTime.Parse("2014-12-31")) .Execute();

Querying with LIKE

var result = client.Select<Property>() .Where(x => x.Name).Like("Name of Property", useFullTextSearch : true) .And(x => x.City).Like("Newtown") .Execute(); 
