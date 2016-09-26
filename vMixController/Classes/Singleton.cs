using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace vMixController.Classes
{
    public class Singleton<T> where T : class
    {
        private static T _instance;

        protected Singleton()
        {
        }

        private static T CreateInstance()
        {
            ConstructorInfo cInfo = typeof(T).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new Type[0],
                new ParameterModifier[0]);

            return (T)cInfo.Invoke(null);
        }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance();
                }

                return _instance;
            }
        }
    }
}
