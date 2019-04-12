using MyTool.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace WabMaker.Web.Helpers
{
    public class CacheHelper
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public void Set(string key, object data, int cacheHour = 6, bool IsAbsolute = true)
        {
            //設定回收       
            //absoluteExpiration:絕對過期時間
            //slidingExpiration:時間內無使用才回收
            CacheItemPolicy policy = new CacheItemPolicy();
            if (IsAbsolute)
            {
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(cacheHour);
            }
            else
            {
                policy.SlidingExpiration = TimeSpan.FromHours(cacheHour);
            }

            //set:快取已存在時,直接覆寫
            //add:快取已存在時,回傳false
            Cache.Set(new CacheItem(key, data), policy);
            //_Log.CreateText("set cache: " + key);
        }

        public T Get<T>(string key)
        {
            var value = Cache[key];

            //if (value != null)
            //{
            //    _Log.CreateText("get cache: " + key);
            //}

            return value == null ? default(T) : (T)value;
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
        }

        public void RemoveAll()
        {
            List<string> cacheKeys = Cache.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                Cache.Remove(cacheKey);
            }
        }
    }
}