using GLTF.Schema;
using nostra.character;
using nostra.core.Post;
using nostra.core.ui;
using nostra.games;
using nostra.models.response;
using nostra.quickplay;
using nostra.quickplay.ui;
using nostra.platform.share;
using nostra.platform.webview;
using nostra.service;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGLTF;
using WebP;

namespace nostra.platform.Core
{
    public class QuickPlayController : MonoBehaviour, PlatformController
    {
        [SerializeField] Transform m_adaptersRoot = null;
        [SerializeField] VerticalScrollSnap scrollSnap;
        [SerializeField] FeedCanvas m_feedCanvas = null;
        [SerializeField] GameCanvas m_gameCanvas = null;
        [SerializeField] GameOverCanvas m_gameOverCanvas = null;
        [SerializeField] WebviewFactory m_webviewFactory = null;
        [SerializeField] NostraShare m_nostraShare = null;
        [SerializeField] GLTFSettings m_gltfSettings = null;
        [SerializeField] OverlayPanel m_overlayPanel = null;
        [SerializeField] QuickPlayType m_quickPlayType;
        
        QuickPlay m_quickPlay = null;
        bool isScrollSnapInitialized = false;
        bool isFirstFocus = true;
        string glbPath;
        string glbFileName;
        Action<string> onExportComplete;

        private void Awake()
        {
            #if UNITY_EDITOR
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            #endif
            
            m_quickPlay = new GameObject("QuickPlay").AddComponent<QuickPlay>();
            m_quickPlay.transform.SetParent(m_adaptersRoot, true);
            m_quickPlay.Initialise(this);

            m_gameCanvas.OnInit(m_quickPlay);
        }
        private void OnApplicationFocus(bool _focus)
        {
            if(_focus == true && isFirstFocus == false && m_quickPlayType == QuickPlayType.QP)
            {
                m_quickPlay?.GetUpdatedIntentData();
            }
            isFirstFocus = false;
        }
        public IWebviewFactory GetWebviewFactory()
        {
            return m_webviewFactory;
        }
        public INostraShare GetNostraShare()
        {
            return m_nostraShare;
        }
        public FeedCanvas GetFeedCanvas()
        {
            return m_feedCanvas;
        }
        public GameCanvas GetGameCanvas()
        {
            return m_gameCanvas;
        }
        public GameOverCanvas GetGameOverCanvas()
        {
            return m_gameOverCanvas;
        }
        public OverlayPanel GetOverlayPanel()
        {
            return m_overlayPanel;
        }
        public void FeedLoaded(int _postCount, bool _isMorePostsAvailable)
        {
            Debug.Log("QPC! FeedLoaded: " + _postCount + " " + _isMorePostsAvailable);
            if(isScrollSnapInitialized == false)
            {
                isScrollSnapInitialized = true;
                scrollSnap.OnScrollStart += (index) => 
                {
                    m_feedCanvas.OnScrollStart(index);
                };
                scrollSnap.OnCardChanged += (cardIndex, postIndex) => 
                {
                    m_feedCanvas.OnCardChanged(cardIndex, postIndex);
                };
                scrollSnap.OnScrollEnd += (finalIndex) => 
                {
                    m_feedCanvas.OnScrollEnd(finalIndex);
                };
                scrollSnap.OnLoadMorePosts += LoadMorePosts;
                scrollSnap.Initialize(_postCount, _isMorePostsAvailable);
            } else
            {
                scrollSnap.AddPosts(_postCount, _isMorePostsAvailable);
            }
        }
        private void LoadMorePosts()
        {
            m_quickPlay.LoadMorePosts();
        }
        public void GotoPostIndex(int _postIndex)
        {
            scrollSnap.GoToPost(_postIndex);
        }
        public void ExportGLB(string _glbFileName, Transform _transform, Action<string> _onExportComplete)
        {
            onExportComplete = _onExportComplete;
            var exportOptions = new ExportContext(m_gltfSettings);
            exportOptions.AfterSceneExport += glbExported;
            var exporter = new GLTFSceneExporter(new[] { _transform }, exportOptions);
            glbPath = Application.persistentDataPath;
            long time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            glbFileName = $"{_glbFileName}_{time}";
            exporter.SaveGLB(glbPath, glbFileName);
        }
        private void glbExported(GLTFSceneExporter exporter, GLTFRoot gltfRoot)
        {
            onExportComplete?.Invoke(System.IO.Path.Combine(glbPath, $"{glbFileName}.glb"));
        }
        public QuickPlayType GetQuickPlayType()
        {
            return m_quickPlayType;
        }
        public List<PostDto> GetDefaultPosts()
        {
            List<PostDto> defaultPosts = new List<PostDto>();
            TextAsset textAsset = Resources.Load<TextAsset>("defaultPost");
            if(textAsset != null)
            {
                defaultPosts.Add(JsonUtility.FromJson<PostDto>(textAsset.text));
            }
            return defaultPosts;
        }
        public void GetWebPTexture2D(Image _image, byte[] _bytes)
        {
            Texture2D texture = Texture2DExt.CreateTexture2DFromWebP(_bytes, lMipmaps: true, lLinear: false, lError: out Error lError);
            if (lError == Error.Success && texture != null && _image != null)
            {
                _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                _image.preserveAspect = true;
                _image.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }
}