using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modules
{
    public abstract class ModuleOwner : MonoBehaviour
    {
        protected Dictionary<Type, IModule> _moduleDict;

        protected virtual void Awake()
        {
            _moduleDict = GetComponentsInChildren<IModule>().ToDictionary(module => module.GetType());
            InitializeComponents();
            AfterInitComponents();
        }
        
        protected virtual void InitializeComponents()
        {
            foreach (IModule module in _moduleDict.Values)    
            {
                module.Initialize(this);
            }    
        }
        
        protected virtual void AfterInitComponents()
        {
            foreach (IAfterInitModule afterInitModule in _moduleDict.Values.OfType<IAfterInitModule>())
            {
                afterInitModule.AfterInit();
            }
        }

        public T GetModule<T>()
        {
            if (_moduleDict.TryGetValue(typeof(T), out IModule module))
            {
                return (T)module;
            }
            
            IModule findModule = _moduleDict.Values.FirstOrDefault(moduleType => moduleType is T);
            if(findModule is T castedModule)
            {
                return castedModule;
            }
            return default;
        }
    }
}