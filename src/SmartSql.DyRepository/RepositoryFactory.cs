﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using SmartSql.Abstractions;
using System.Collections;

namespace SmartSql.DyRepository
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private IDictionary<Type, object> _cachedRepository = new Dictionary<Type, object>();

        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;
        private readonly IRepositoryBuilder _repositoryBuilder;

        public RepositoryFactory(IRepositoryBuilder RepositoryBuilder)
        {
            Init();
            this._repositoryBuilder = RepositoryBuilder;
        }
        private void Init()
        {
            string assemblyName = "SmartSql.RepositoryImpl" + this.GetHashCode();
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName
            {
                Name = assemblyName
            }, AssemblyBuilderAccess.Run);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName + ".dll");
        }

        public object CreateInstance(Type interfaceType, ISmartSqlMapper smartSqlMapper)
        {
            if (!_cachedRepository.ContainsKey(interfaceType))
            {
                lock (this)
                {
                    var implType = _repositoryBuilder.BuildRepositoryImpl(interfaceType);
                    var obj = Activator.CreateInstance(implType, new object[] { smartSqlMapper });
                    _cachedRepository.Add(interfaceType, obj);
                }
            }
            return _cachedRepository[interfaceType];
        }


        public T CreateInstance<T>(ISmartSqlMapper smartSqlMapper)
        {
            var interfaceType = typeof(T);
            return (T)CreateInstance(interfaceType, smartSqlMapper);
        }
    }
}