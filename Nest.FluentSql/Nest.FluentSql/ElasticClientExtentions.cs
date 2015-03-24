namespace Nest.FluentSql
{
    public static class ElasticClientExtentions
    {
        /// <summary>
        /// Extension method on ElasticClient to perform SQL likes search
        /// </summary>
        /// <typeparam name="T">Class Type to perform search on</typeparam>
        /// <param name="client">Configured ElasticClient</param>
        /// <returns>FluentSelect</returns>
        public static FluentSelect<T> Select<T>(this ElasticClient client) where T : class 
        {
            return new FluentSelect<T>(client);
        }
    }
}
