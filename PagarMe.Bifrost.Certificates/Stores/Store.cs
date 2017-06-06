﻿using NLog;
using PagarMe.Bifrost.Certificates.Generation;
using System;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Stores
{
    abstract class Store
    {
        static Store()
        {
            Instance = getInstance();
        }

        private static Store getInstance()
        {
            if (TLSConfig.IsUnix)
                return new UnixStore();

            return new WindowsStore();
        }

        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static Store Instance { get; private set; }

        public abstract X509Certificate2 GetCertificate(String subject, String issuer, StoreName storeName);
        public abstract void AddCertificate(X509Certificate2 ca, X509Certificate2 tls);
    }


}