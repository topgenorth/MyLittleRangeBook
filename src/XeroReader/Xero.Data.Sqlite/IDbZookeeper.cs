// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace net.opgenorth.xero.data.sqlite
{
    public interface IDbZookeeper
    {
        string ConnectionString { get; }
        string SqliteFile { get; }

        /// <summary>
        ///     Runs the migrations on the <b>.sqlite</b> file.
        /// </summary>
        void UpdateDatabase();

        /// <summary>
        ///     Will delete the <b>.sqlite</b> file if it exists, create a new one, and apply migrations.
        /// </summary>
        void CreateDatabase();

        int GetHashCode();
        string ToString();
    }
}
