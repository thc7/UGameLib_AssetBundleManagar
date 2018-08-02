using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Libs
{
    public delegate AssetBundleLoader LoaderCreate(string path);
    //AssetBundle 加载完成回调
    public delegate void OnLoadAssetBundle(string path, AssetBundle assetBundle);
    public delegate void OnLoadCmpCallBack();
    /// <summary>
    /// AssetBundle加载器抽象！
    /// </summary>
    public abstract class AssetBundleLoader
    {
        public string name;
        public AssetBundle assetBundle;
        uint _referenceCount = 0;

        public uint referenceCount
        {
            get
            {
                return _referenceCount;
            }
        }
        public OnLoadAssetBundle onLoadAssetBundle;

        public uint Retain()
        {
            return ++_referenceCount;
        }

        public uint Release()
        {
            return --_referenceCount;
        }

        virtual public bool IsCmp()
        {
            return false;
        }

        virtual public float Progress()
        {
            return 0;
        }

        public void AddCallBack(OnLoadAssetBundle onLoadAssetBundlep)
        {
            if (onLoadAssetBundlep == null)
            {
                return;
            }
            if (onLoadAssetBundle == null)
            {
                onLoadAssetBundle = onLoadAssetBundlep;
            }
            else
            {
                Delegate[] invocationList = onLoadAssetBundle.GetInvocationList();

                for (int i = invocationList.Length - 1; i >= 0; i--)
                {
                    System.Delegate curDelegate = invocationList[i];
                    //同一个ab 同一个对象 同一个方法 只能监听一次
                    if (curDelegate.Target == onLoadAssetBundlep.Target && curDelegate.Method == onLoadAssetBundlep.Method)
                    {
                        Debug.LogWarningFormat("Target {0}, Method{1},is already in AssetBundleLoader {2}", onLoadAssetBundlep.Target, onLoadAssetBundlep.Method, name);
                        return;
                    }
                }
                onLoadAssetBundle += onLoadAssetBundlep;
            }
        }

        virtual public void CallBack()
        {
            if(_referenceCount == 0)
            {
                onLoadAssetBundle = null;
                assetBundle.Unload(false);
                assetBundle = null;

                return;
            }

            if (onLoadAssetBundle != null)
            {
                onLoadAssetBundle(name, assetBundle);
                onLoadAssetBundle = null;
                assetBundle = null;
                _referenceCount = 0;
            }
        }

        virtual public void Stop()
        {
            Clear();
        }

        virtual public void Clear()
        {
            if (assetBundle)
                assetBundle.Unload(false);
            
            onLoadAssetBundle = null;
            assetBundle = null;
            _referenceCount = 0;
        }

        virtual public IEnumerator LoadAsync(string assetBundleName)
        {
            yield return null;
        }
    }
    /// <summary>
    /// www 加载方式
    /// </summary>
    public class AssetBundleWebRequestLoader : AssetBundleLoader
    {
        public UnityWebRequest download;
        bool isDone;

        override public IEnumerator LoadAsync(string assetBundleName)
        {
            download = UnityWebRequest.Get(PathTools.GetAssetPath(assetBundleName));

            yield return download.SendWebRequest();
            if (!string.IsNullOrEmpty(download.error))
            {
                Debug.LogError(download.error);
            }
            else if (download.responseCode == 200)
            {
                assetBundle = AssetBundle.LoadFromMemory(download.downloadHandler.data);
                isDone = true;
            }
        }

        override public bool IsCmp()
        {
            return isDone;
        }

        override public float Progress()
        {
            if (download == null) return 0;
            return download.downloadProgress;
        }

        override public void CallBack()
        {
            base.CallBack();
            if (download != null)
            {
                download.Dispose();
                download = null;
            }
        }
    }

    /// <summary>
    /// www 加载方式
    /// </summary>
    public class AssetBundleWWWLoader : AssetBundleLoader
    {
        public WWW download;
        bool isDone;

        override public IEnumerator LoadAsync(string assetBundleName)
        {
            download = new WWW(PathTools.GetAssetPath(assetBundleName));
            yield return download;

            if (!string.IsNullOrEmpty(download.error))
            {
                Debug.LogError(download.error);
            }

            assetBundle = download.assetBundle;

            isDone = true;
        }

        override public bool IsCmp()
        {
            return isDone;
        }

        override public float Progress()
        {
            if (download == null) return 0;
            return download.progress;
        }

        override public void CallBack()
        {
            base.CallBack();
            if (download != null)
            {
                download.Dispose();
                download = null;
            }
        }
    }
    /// <summary>
    /// LoadFromFileAsync 加载方式
    /// </summary>
    public class AssetBundleLoadFromFileLoader : AssetBundleLoader
    {
        AssetBundleCreateRequest r;
        override public IEnumerator LoadAsync(string assetBundleName)
        {
            r = AssetBundle.LoadFromFileAsync(PathTools.GetAssetPathForLoadPath(assetBundleName));
            yield return r;

            assetBundle = r.assetBundle;
        }

        override public bool IsCmp()
        {
            if (r == null) return false;
            return r.isDone;
        }

        override public float Progress()
        {
            if (r == null) return 0;
            return r.progress;
        }
    }
    /// <summary>
    /// Asset bundle create from memory. 解密方式
    /// </summary>
    public class AssetBundleCreateFromMemory : AssetBundleLoader
    {
        WWW download;
        bool isDone;

        override public IEnumerator LoadAsync(string assetBundleName)
        {

            download = new WWW(PathTools.GetAssetPath(assetBundleName));
            yield return download;

            byte[] encrypedData = download.bytes;
            byte[] decryptedData = DecryptionMethod(encrypedData);//解密函数
            AssetBundleCreateRequest abcr = AssetBundle.LoadFromMemoryAsync(decryptedData);

            yield return abcr;

            assetBundle = abcr.assetBundle;

            isDone = true;
        }

        byte[] DecryptionMethod(byte[] encrypedData)
        {
            //解密算法
            byte[] decryptedData = encrypedData;
            return decryptedData;
        }

        override public bool IsCmp()
        {
            if (download == null) return false;
            return isDone;
        }

        override public float Progress()
        {
            if (download == null) return 0;
            return download.progress;
        }

        override public void CallBack()
        {
            base.CallBack();
            if (download != null)
            {
                download.Dispose();
                download = null;
            }
        }
    }

    /// <summary>
    /// AssetBundle 实例 管理 引用次数
    /// </summary>
    public class AssetBundleInstance
    {
        public string name;
        public AssetBundle assetBundle;
        uint referenceCount;

        public uint Retain()
        {
            return ++referenceCount;
        }

        public uint Release()
        {
            return --referenceCount;
        }

        public void SetReferenceCount(uint count)
        {
            referenceCount = count;
        }

        public uint GetReferenceCount()
        {
            return referenceCount;
        }

        public void Unload(bool isUnloadAsset = false)
        {
            if (assetBundle)
            {
                assetBundle.Unload(isUnloadAsset);
                assetBundle = null;
            }
            else
            {
                Debug.LogErrorFormat("{0} 在执行 Unload 前已经被卸载!", name);
            }
        }
    }

    /// <summary>
    /// AssetBundle 管理器 
    /// 封装AssetBundle 加载过程
    /// 提供加载队列
    /// 提供存取接口
    /// </summary>
    public class AssetBundleManagar : MonoBehaviour
    {
        private static AssetBundleManagar instance;
        public static AssetBundleManagar getInstance()
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject("AssetBundleManagar");
                DontDestroyOnLoad(gameObject);
                instance = gameObject.AddComponent<AssetBundleManagar>();
            }
            return instance;
        }
        public static AssetBundleManagar initForGameObject(GameObject dontDestroyOnLoadGameObject)
        {
            if (instance == null)
            {
                instance = dontDestroyOnLoadGameObject.AddComponent<AssetBundleManagar>();
            }
            return instance;
        }

        public static bool debugLog = true;


        Dictionary<string, AssetBundleInstance> cache = new Dictionary<string, AssetBundleInstance>();

        Queue<AssetBundleLoader> waiting = new Queue<AssetBundleLoader>();

        Dictionary<string, AssetBundleManifest> assetBundleManifestDic = new Dictionary<string, AssetBundleManifest>();

        AssetBundleManifest[] assetBundleManifestArr;

        AssetBundleLoader curAssetBundleLoader;

        AssetBundleManifest manifest;

        public LoaderCreate loaderCreate;

        bool isStop;

        void Awake()
        {
            //加载主 manifest
            //LoadAssetBundleManifest();
        }

        AssetBundleLoader defaultLoaderCreate(string path)
        {
            AssetBundleLoader assetBundleLoader;

            if (Application.platform == RuntimePlatform.Android)
                assetBundleLoader = new AssetBundleWWWLoader();
            else
                assetBundleLoader = new AssetBundleLoadFromFileLoader();

            return assetBundleLoader;
        }

        public  Dictionary<string, AssetBundleInstance> AssetBundleInstanceCache
        {
            get
            {
                return cache;
            }
        }

        public void ClearCache(bool isUnloadAsset = false)
        {
            foreach(AssetBundleInstance assetBundleInstance in cache.Values)
            {
                assetBundleInstance.Unload(isUnloadAsset);
                Debug.LogWarningFormat("缓存卸载:{0}",assetBundleInstance.name);
            }
            cache.Clear();
        }

        public void LogCacheInfo()
        {
            foreach (AssetBundleInstance assetBundleInstance in cache.Values)
            {
                Debug.LogWarningFormat("缓存:{0},依赖引用:{1}", assetBundleInstance.name,assetBundleInstance.GetReferenceCount());
            }
        }

        public void ClearWaiting()
        { 
            waiting.Clear();
        }

        public void Stop()
        {
            isStop = true;
        }

        public void Run()
        {
            isStop = false;
        }
        // 加载主 Manifest
        public void LoadAssetBundleManifest(OnLoadCmpCallBack callBack = null)
        {
            if (manifest != null)
                return;

            LoadOne("StreamingAssets", delegate (string path, AssetBundle assetBundle)
            {
                manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                //assetBundle.Unload(false);
                if (callBack != null)
                {
                    callBack();
                }
            });
        }

        /// <summary>
        /// Loads the asset bundle manifest add.
        /// 增加依赖列表文件,美术工程
        /// </summary>
        /// <param name="streamingAssetsFileName">Streaming assets file name.</param>
        public void LoadAssetBundleManifestAdd(string streamingAssetsFileName, OnLoadCmpCallBack callBack = null)
        {
            if (assetBundleManifestDic.ContainsKey(streamingAssetsFileName))
            {
                Debug.LogFormat("manifest {0} 已缓存...", streamingAssetsFileName);
                return;
            }

            LoadOne(streamingAssetsFileName, delegate (string path, AssetBundle assetBundle)
            {
                AssetBundleManifest manifestAdd = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

                assetBundleManifestDic.Add(path, manifestAdd);
                assetBundleManifestArr = new AssetBundleManifest[assetBundleManifestDic.Values.Count];
                assetBundleManifestDic.Values.CopyTo(assetBundleManifestArr, 0);

                if (callBack != null)
                {
                    callBack();
                }
            });

        }
        /// <summary>
        /// 获取依赖列表
        /// </summary>
        /// <param name="abName">Ab name.</param>
        public string[] GetDependencies(string abName)
        {
            string[] dependencies = manifest.GetAllDependencies(abName);

            if(dependencies !=null && dependencies.Length > 0)
            {
                return dependencies;
            }

            for (int i = 0; assetBundleManifestArr != null && i < assetBundleManifestArr.Length; i++)
            {
                dependencies = assetBundleManifestArr[i].GetAllDependencies(abName);

                if (dependencies != null && dependencies.Length > 0)
                {
                    return dependencies;
                }
            }
            return null;
        }
        /// <summary>
        /// Loads the and dependencies.
        /// 加载 AssetBundle 和其依赖的 AssetBundle
        /// </summary>
        /// <param name="abName">Ab name.</param>
        /// <param name="onLoadAssetBundle">On load asset bundle.</param>
        protected void LoadAndDependencies(string abName, OnLoadAssetBundle onLoadAssetBundle)
        {
            int dependenciesCount = 0;

            if (manifest != null)
            {
                string[] dependencies = manifest.GetAllDependencies(abName);
                dependenciesCount = dependencies.Length;

                for (int i = dependenciesCount - 1; i >= 0; i--)
                {
                    LoadOne(dependencies[i], null);
                }
            }

            if (dependenciesCount == 0)
            {
                for (int i = 0; assetBundleManifestArr != null && i < assetBundleManifestArr.Length; i++)
                {
                    string[] dependencies = assetBundleManifestArr[i].GetAllDependencies(abName);
                   
                    for (int j = dependenciesCount - 1; j >= 0; j--)
                    {
                        LoadOne(dependencies[j], null);
                    }
                    if (dependencies.Length > 0)
                    {
                        LoadOne(abName, onLoadAssetBundle);
                        return;
                    }
                    /*
					dependenciesCount = LoadDependencies (abName, assetBundleManifestArr [i], onLoadAssetBundle);
					//在附加库中查找依赖
					if (dependenciesCount > 0) {
						break;
					}*/
                }
            }

            LoadOne(abName, onLoadAssetBundle);
        }
        /// <summary>
        /// 加载 AssetBundle 和其依赖的 AssetBundle 外部调用接口
        /// </summary>
        /// <param name="abName">Ab name.</param>
        /// <param name="onLoadAssetBundle">On load asset bundle.</param>
        public void Load(string abName, OnLoadAssetBundle onLoadAssetBundle)
        {
            LoadAndDependencies(abName, onLoadAssetBundle);
        }
        /// <summary>
        /// Loads the dependencies.
        /// 加载和子依赖 
        /// 引用计数加 1 
        /// </summary>
        /// <param name="abName">Ab name.</param>
        protected int LoadDependencies(string abName, AssetBundleManifest assetBundleManifest, OnLoadAssetBundle onLoadAssetBundle = null)
        {
            string[] dependencies = assetBundleManifest.GetAllDependencies(abName);
            //加载子依赖

            for (int i = 0; i < dependencies.Length; i++)
            {
                int dependencieLength = LoadDependencies(dependencies[i], assetBundleManifest);
                Debug.LogWarningFormat("assetBundle {0} ,dependencie {1} ,dependencieLength {2} ", abName, dependencies[i], dependencieLength);
            }

            //1. 检测 AssetBundle 缓存
            AssetBundleInstance assetBundleInstance;
            cache.TryGetValue(abName, out assetBundleInstance);
            if (assetBundleInstance != null)
            {
                //当前 引用计数加 1
                assetBundleInstance.Retain();
                Debug.LogFormat("{0} 已缓存...", abName);
                if (onLoadAssetBundle != null)
                {
                    onLoadAssetBundle(assetBundleInstance.name, assetBundleInstance.assetBundle);
                }
                return dependencies.Length;
            }
            //2. 检查 是否在等待队列里
            AssetBundleLoader[] waitings = waiting.ToArray();
            AssetBundleLoader curWaitingAssetBundleLoader;
            for (int i = 0; i < waitings.Length; i++)
            {
                curWaitingAssetBundleLoader = waitings[i];
                if (curWaitingAssetBundleLoader.name.Equals(abName))
                {
                    //当前 引用计数加 1
                    curWaitingAssetBundleLoader.Retain();
                    curWaitingAssetBundleLoader.AddCallBack(onLoadAssetBundle);
                    Debug.LogFormat("{0} 正在加载等待中...", curWaitingAssetBundleLoader.name);
                    return dependencies.Length;
                }
            }
            //3. 检测 同名资源是否正在加载中   
            if (curAssetBundleLoader != null && curAssetBundleLoader.name.Equals(abName))
            {
                //当前 引用计数加 1
                curAssetBundleLoader.Retain();
                curAssetBundleLoader.AddCallBack(onLoadAssetBundle);

                Debug.LogFormat("{0} 正在加载中...", curAssetBundleLoader.name);
                return dependencies.Length;
            }
            //4. 创建加载器
            AssetBundleLoader assetBundleLoader;//= new AssetBundleWWWLoader();

            if(loaderCreate == null)
            {
                assetBundleLoader = defaultLoaderCreate(abName);
            }
            else
            {
                assetBundleLoader = loaderCreate(abName);
            }
           
            assetBundleLoader.name = abName;
            assetBundleLoader.onLoadAssetBundle = onLoadAssetBundle;
            assetBundleLoader.Retain();
            //5. 开始加载或加入等待队列
            StartOrWait(assetBundleLoader);

            return dependencies.Length;
        }
        /// <summary>
        /// 加载并缓存
        /// 引用计数加 1 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onLoadAssetBundle"></param>
        public void LoadOne(string abName, OnLoadAssetBundle onLoadAssetBundle)
        {
            //1.检测 AssetBundle 缓存
            AssetBundleInstance assetBundleInstance;
            cache.TryGetValue(abName, out assetBundleInstance);
            if (assetBundleInstance != null)
            {
                //当前 引用计数加 1
                assetBundleInstance.Retain();
                Debug.LogFormat("{0} 已缓存...", abName);

                if (onLoadAssetBundle != null)
                {
                    onLoadAssetBundle(abName, assetBundleInstance.assetBundle);
                }
                return;
            }
            //2. 检查 是否在等待队列里
            AssetBundleLoader[] waitings = waiting.ToArray();
            AssetBundleLoader curWaitingAssetBundleLoader;
            for (int i = 0; i < waitings.Length; i++)
            {
                curWaitingAssetBundleLoader = waitings[i];
                if (curWaitingAssetBundleLoader.name.Equals(abName))
                {
                    //当前 引用计数加 1
                    curWaitingAssetBundleLoader.Retain();
                    curWaitingAssetBundleLoader.AddCallBack(onLoadAssetBundle);
                    Debug.LogFormat("{0} 正在加载等待中...", curWaitingAssetBundleLoader.name);
                    return;
                }
            }
            //3. 同名资源是否正在加载中   
            if (curAssetBundleLoader != null && curAssetBundleLoader.name.Equals(abName))
            {
                //System.Delegate[] invocationList = curAssetBundleLoader.onLoadAssetBundle.GetInvocationList();
                //当前 引用计数加 1
                curAssetBundleLoader.Retain();
                curAssetBundleLoader.AddCallBack(onLoadAssetBundle);
                //System.Delegate[] invocationList1 = curAssetBundleLoader.onLoadAssetBundle.GetInvocationList();
                return;
            }
            //4. 创建加载器
            AssetBundleLoader assetBundleLoader;//= new AssetBundleWWWLoader();

            if (loaderCreate == null)
            {
                assetBundleLoader = defaultLoaderCreate(abName);
            }
            else
            {
                assetBundleLoader = loaderCreate(abName);
            }

            assetBundleLoader.name = abName;
            assetBundleLoader.onLoadAssetBundle = onLoadAssetBundle;
            assetBundleLoader.Retain();
            //5. 开始加载或加入等待队列
            StartOrWait(assetBundleLoader);
        }

        /// <summary>
        /// 释放一个 ab 
        /// </summary>
        /// <param name="name"></param>
        public uint Release(string abName, AssetBundleManifest assetBundleManifest = null)
        {
            bool isManifest = (assetBundleManifest == null && manifest != null);
            int dependenciesCount = 0;
            if (isManifest)
            {
                assetBundleManifest = manifest;
            }

            uint referenceCount = ReleaseOne(abName);

            if (referenceCount > 0)
            {
                return referenceCount;
            }

            string[] dependencies = assetBundleManifest.GetAllDependencies(abName);
            dependenciesCount = dependencies.Length;
            //manifest 子依赖
            if (dependencies.Length > 0)
            {
                //释放子依赖
                for (int i = 0; i < dependencies.Length; i++)
                {
                    if (debugLog)
                        Debug.LogWarningFormat("assetBundle {0} dependencies {1}", abName, dependencies[i]);
                    ReleaseOne(dependencies[i]);
                }
            }
            else if (!isManifest && assetBundleManifestArr != null)
            {
                //释放子依赖 在外部manifest列表中
                for (int i = 0; i < assetBundleManifestArr.Length; i++)
                {
                    //在附加库中查找依赖
                    AssetBundleManifest curAssetBundleManifest = assetBundleManifestArr[i];
                    string[] curDependencies = curAssetBundleManifest.GetAllDependencies(abName);
                    dependenciesCount = curDependencies.Length;

                    if (dependenciesCount > 0)
                    {
                        for (int j = 0; j < dependenciesCount; j++)
                        {
                            ReleaseOne(abName);
                        }
                        break;
                    }
                }
            }
          
            return referenceCount;
        }

        uint ReleaseOne(string abName)
        {
            //1.检测正在加载中的
            if(curAssetBundleLoader!=null)
            {
                if(curAssetBundleLoader.name.Equals(abName))
                {
                    curAssetBundleLoader.Release();
                    return curAssetBundleLoader.referenceCount;
                }   
            }
            //2. 检查 是否在等待队列里
            AssetBundleLoader[] waitings = waiting.ToArray();
            AssetBundleLoader curWaitingAssetBundleLoader;
            for (int i = 0; i < waitings.Length; i++)
            {
                curWaitingAssetBundleLoader = waitings[i];
                if (curWaitingAssetBundleLoader.name.Equals(abName))
                {
                    curWaitingAssetBundleLoader.Release();
                    return curWaitingAssetBundleLoader.referenceCount;
                }
            }
            //3.检测缓存
            AssetBundleInstance assetBundleInstance = null;
            cache.TryGetValue(abName, out assetBundleInstance);

            if (assetBundleInstance == null)
            {
                Debug.LogErrorFormat("AssetBundleManagar cache 中没有 {0} ", abName);
                return 0;
            }
            //引用计数减 1 
            if (assetBundleInstance.Release() == 0)
            {
                assetBundleInstance.Unload();
                cache.Remove(abName);
                if (debugLog)
                    Debug.LogErrorFormat("释放 {0}", abName);
                return 0;
            }
            if (debugLog)
            {
                Debug.LogErrorFormat("引用减 1 {0} ReferenceCount {1}", abName, assetBundleInstance.GetReferenceCount());
            }

            return assetBundleInstance.GetReferenceCount();
        }
        /// <summary>
        /// 强制移除 不检查引用
        /// </summary>
        /// <param name="name"></param>
        public void Rem(string name)
        {
            AssetBundleInstance assetBundleInstance = null;
            cache.TryGetValue(name, out assetBundleInstance);
            if (assetBundleInstance != null)
            {
                assetBundleInstance.assetBundle.Unload(true);
                cache.Remove(name);
            }
        }
        /// <summary>
        /// Starts the or wait.
        /// </summary>
        void StartOrWait(AssetBundleLoader assetBundleLoader)
        {
            //当前正在加载中
            if (IsBusy())
            {
                //加入等待队列
                waiting.Enqueue(assetBundleLoader);
            }
            else
            {
                //开始加载！
                curAssetBundleLoader = assetBundleLoader;
                StartCoroutine(curAssetBundleLoader.LoadAsync(assetBundleLoader.name));
            }
        }
        /// <summary>
        /// 加载器是否处于忙碌状态
        /// </summary>
        /// <returns><c>true</c> if this instance is busy; otherwise, <c>false</c>.</returns>
        bool IsBusy()
        {
            return curAssetBundleLoader != null;
        }
        /// <summary>
        /// 是否有加载器正在运行
        /// </summary>
        /// <returns><c>true</c> if this instance is running; otherwise, <c>false</c>.</returns>
        bool IsRunning()
        {
            return IsBusy();
        }
        /// <summary>
        /// 执行下一个加载任务
        /// </summary>
        void Next()
        {
            //检测队列
            if (curAssetBundleLoader == null && waiting.Count > 0)
            {
                //弹出排队目标
                curAssetBundleLoader = waiting.Dequeue();
                //检测引用
                if(curAssetBundleLoader.referenceCount <= 0)
                {
                    curAssetBundleLoader.Clear();
                    curAssetBundleLoader = null;
                    return;
                }
                //检测缓存
                AssetBundleInstance assetBundleInstance;
                cache.TryGetValue(curAssetBundleLoader.name, out assetBundleInstance);
                if (assetBundleInstance != null)
                {
                    curAssetBundleLoader.onLoadAssetBundle(curAssetBundleLoader.name, assetBundleInstance.assetBundle);
                    curAssetBundleLoader = null;
                    return;
                }

                //启动加载
                StartCoroutine(curAssetBundleLoader.LoadAsync(curAssetBundleLoader.name));
            }
        }
        /// <summary>
        /// 加入缓存 Caches the new one.
        /// </summary>
        /// <param name="assetBundleLoader">Asset bundle loader.</param>
        void CacheNewOne(AssetBundleLoader assetBundleLoader)
        {
            //检测缓存
            AssetBundleInstance assetBundleInstance;
            cache.TryGetValue(curAssetBundleLoader.name, out assetBundleInstance);
            if (assetBundleInstance == null)
            {
                assetBundleInstance = new AssetBundleInstance();
                assetBundleInstance.name = assetBundleLoader.name;
                assetBundleInstance.assetBundle = curAssetBundleLoader.assetBundle;
                assetBundleInstance.SetReferenceCount(curAssetBundleLoader.referenceCount);
                //添加到缓存
                cache.Add(curAssetBundleLoader.name, assetBundleInstance);

                Debug.LogWarningFormat("缓存 {0}", assetBundleInstance.name);
            }
            else
            {
                Debug.LogErrorFormat("{0} 重复加载!", assetBundleInstance);
            }
        }

        /// <summary>
        /// 检查当前运行 curAssetBundleLoader 是否加载完成
        /// </summary>
        void Check()
        {
            if(isStop)
            {
                curAssetBundleLoader.Stop();

                curAssetBundleLoader = null;

                Debug.LogWarningFormat(" Stop in Check() !");
            }

            if (curAssetBundleLoader.IsCmp())
            {
                if (curAssetBundleLoader.assetBundle == null)
                {
                    Debug.LogErrorFormat("AssetBundle {0} 加载失败!", curAssetBundleLoader.name);
                    //重置流
                    curAssetBundleLoader = null;
                    return;
                }

                if (curAssetBundleLoader.referenceCount > 0)
                {
                    CacheNewOne(curAssetBundleLoader);
                    //回调完成委托
                    curAssetBundleLoader.CallBack();

                    if (debugLog)
                    {
                        Debug.LogFormat(" AssetBundleLoader {0}, 加载完成!,队列总数 {1}", curAssetBundleLoader.name, waiting.Count);
                    }
                }
                else
                {
                    curAssetBundleLoader.Clear();

                    Debug.LogWarningFormat(" AssetBundleLoader {0}, referenceCount = 0 Clear !",curAssetBundleLoader.name);
                }
                //重置流
                curAssetBundleLoader = null;
            }
            else
            {
                //正在加载中！
                //Debug.Log("curAssetBundleLoader = " + curAssetBundleLoader.progress());
            }
        }

        void LateUpdate()
        {
            if (IsRunning())
            {
                Check();
            }
            else if (!isStop)
            {
                //加载下一个
                Next();
            }
        }

    }//class end

    /// <summary>
    /// 简化调用接口
    /// </summary>
    public class ABM : AssetBundleManagar
    {
        public static AssetBundleManagar I
        {
            get
            {
                return AssetBundleManagar.getInstance();
            }
        }
    }
    /// <summary>
    ///   Dictionary<string,string> abDic = new Dictionary<string, string>();
    ///   Libs.ManifestFileTools.ReadAssetsame2AssetBundleInDic("StreamingAssets_loadAb"+"_AssetsName2AssetBundleAll.txt" ,abDic);
    ///   string ab = abDic["Cube"];
    ///   Debug.Log(ab);
    /// </summary>
    public class ManifestFileTools
    {
        public static void ReadAssetsName2AssetBundleInDicFullPath(string fullPath, Dictionary<string, string> abDic)
        {
            string fileText = System.IO.File.ReadAllText(fullPath).Trim();
            string[] allLines = fileText.Split('\n');
            string curLine;
            int curIndex;

            for (int i = 0; i < allLines.Length; i++)
            {
                curLine = allLines[i];
                curIndex = curLine.IndexOf('=');
                if (curIndex < 0)
                {
                    Debug.LogErrorFormat("ReadAssetsame2AssetBundleInDic Error Line {0}", curLine);
                    continue;
                }
                else
                {
                    abDic.Add(curLine.Substring(0, curIndex), curLine.Substring(curIndex + 1));
                }
            }

        }

        /// <summary>
        /// Reads the assetsame  asset bundle in dic.
        /// </summary>
        /// <param name="streamingAssetsFileName">Streaming assets file name.</param>
        /// <param name="abDic">Ab dic.</param>
        public static void ReadAssetsName2AssetBundleInDic(string streamingAssetsFileName, Dictionary<string, string> abDic)
        {
            ReadAssetsName2AssetBundleInDicFullPath(PathTools.GetAssetPathForLoadPath(streamingAssetsFileName), abDic);
        }

        public static void CreateAssetsName2AssetBundle(string streamingAssetsFileName)
        {
            string filePath = PathTools.GetAssetPathForLoadPath(streamingAssetsFileName);
            AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
            AssetBundleManifest manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            string[] assetBundles = manifest.GetAllAssetBundles();

            string path = PathTools.GetAssetPathForLoadPath(streamingAssetsFileName + "_AssetsName2AssetBundleAll.txt");
            System.IO.StreamWriter sw = System.IO.File.CreateText(path);

            foreach (string abName in assetBundles)
            {
                if (abName.StartsWith("StreamingAssets", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] namesArr = GetManifestAssetsNames(abName);

                foreach (string line in namesArr)
                {
                    /*
                    if (line.Equals("Assets/Map/Z!M.O.B.A Environment Art") || line.StartsWith("Assets/Map/Z!M.O.B.A Environment Art"))
                    {
                        Debug.Log("");
                    }
                    */
                    sw.WriteLine(line);
                    sw.Flush();
                }
            }

            sw.Close();
        }

        public static string[] GetManifestAssetsNames(string assetBundleName)
        {
            string path = PathTools.GetAssetPathForLoadPath(assetBundleName + ".manifest");
            string fileText = System.IO.File.ReadAllText(path);
            string allLines = fileText.Substring(fileText.LastIndexOf("Assets:\n- ", StringComparison.Ordinal) + "Assets:\n- ".Length,
                                                 fileText.LastIndexOf("Dependencies:", StringComparison.Ordinal) + 1 - "Dependencies:".Length - fileText.LastIndexOf("Assets:", StringComparison.Ordinal) + 1);
            string[] lineArr = allLines.Trim().Split('\n');
            string[] assetsNameLineArr = new string[lineArr.Length * 2];
           
            string curLine;
            for (int i = 0; i < lineArr.Length; i++)
            {
                curLine = lineArr[i];
                assetsNameLineArr[i * 2] = curLine.Substring(curLine.LastIndexOf("/", StringComparison.Ordinal) + 1, curLine.LastIndexOf(".", StringComparison.Ordinal) - curLine.LastIndexOf("/", StringComparison.Ordinal) - 1) + "=" + assetBundleName;
                //assetsPathLineArr[i] = curLine.Replace("-").Trim();
                assetsNameLineArr[i * 2 + 1] = curLine.Replace("-", "").Trim() + "=" + assetBundleName;
            }

            return assetsNameLineArr;
        }
    }
}