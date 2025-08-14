using DG.Tweening;
using EasyButtons;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace nostra.booboogames.Tanuki
{
    
    public class GameManager : MonoBehaviour
    {
        [SerializeField] Button RestartBtn;
        [SerializeField] ColorManager colorManager;


        private void Start()
        {

            Application.targetFrameRate = 90;
            RestartBtn.onClick.AddListener(() => 
            {
                SceneManager.LoadScene(0);
            });

            GameOverRstBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(0);
            });

            originalPos = DriftTempScore.transform.position;
        }


        #region Coins Manager

        [Header("CoinsManager")]
        [SerializeField] TextMeshProUGUI CoinsText;
        [SerializeField] int currentCoinCount;
        [SerializeField] Transform TargetTempscore;
       
        [SerializeField] TextMeshProUGUI TempScoreText;

        [SerializeField] GearBox gearBox;


        [Header("Drift obj")]
        [SerializeField] TextMeshProUGUI DriftTempScore;
        Vector3 originalPos;

        private int lastMilestone = 0; // Last milestone reached
        [SerializeField] int milestoneStep = 2000; // Step between milestones

        public void IncreaseCoins(int coinsAdd = 1)
        {
            int oldValue = currentCoinCount;
            int newValue = currentCoinCount + coinsAdd;
            currentCoinCount = newValue;

            int nextMilestone = lastMilestone + milestoneStep;
            if (currentCoinCount >= nextMilestone)
            {
                lastMilestone = nextMilestone;
                colorManager.NextColorSet();
            }

            // Save original position and color
            Vector3 originalPos = TempScoreText.transform.position;
            Color originalColor = TempScoreText.color;

            // Set initial text and fully transparent
            TempScoreText.text = "+" + coinsAdd;
            TempScoreText.transform.position = originalPos;
            TempScoreText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            // Create a sequence
            Sequence seq = DOTween.Sequence();

            seq.Append(TempScoreText.DOFade(1, 0.2f)) // Fade in
               .Append(TempScoreText.transform.DOMove(TargetTempscore.position, 0.5f).SetEase(Ease.InQuad)) // Move
               .Join(TempScoreText.DOFade(0, 0.5f)) // Fade out during move
               .AppendCallback(() =>
               {
                   // Coin value increment
                   CoinsText.transform.DOKill();
                   CoinsText.transform.localScale = Vector3.one;

                   float baseSpeed = 0.05f;
                   float duration = Mathf.Clamp(coinsAdd * baseSpeed, 0.2f, 1.2f);

                   DOTween.To(() => oldValue, x =>
                   {
                       oldValue = x;
                       CoinsText.text = x.ToString();
                   }, newValue, duration).SetEase(Ease.OutQuad);

                   CoinsText.transform.DOScale(Vector3.one * 1.3f, 0.1f).OnComplete(() =>
                   {
                       CoinsText.transform.DOScale(Vector3.one, 0.2f);
                       gearBox.SpinWheelOn = false;
                       gearBox.Resetpos();
                   });
               })
               .AppendInterval(0.1f)
               .AppendCallback(() =>
               {
                   // Reset position and alpha
                   TempScoreText.transform.position = originalPos;
                   TempScoreText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
               });
        }


        public void IncreaseCoinDrift(int coinsAdd = 1)
        {
            int oldValue = currentCoinCount;
            int newValue = currentCoinCount + coinsAdd;
            currentCoinCount = newValue;

            // Save original position and color
            //Vector3 originalPos = DriftTempScore.transform.position;
            Color originalColor = DriftTempScore.color;

            // Set initial text and fully transparent
            DriftTempScore.text = "+" + coinsAdd;
            DriftTempScore.transform.position = originalPos;
            DriftTempScore.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            // Create a sequence
            Sequence seq = DOTween.Sequence();

            seq.Append(DriftTempScore.DOFade(1, 0.2f)) // Fade in
               .Append(DriftTempScore.transform.DOMove(TargetTempscore.position, 0.5f).SetEase(Ease.InQuad)) // Move
               .Join(DriftTempScore.DOFade(0, 0.5f)) // Fade out during move
               .AppendCallback(() =>
               {
                   // Coin value increment
                   CoinsText.transform.DOKill();
                   CoinsText.transform.localScale = Vector3.one;

                   float baseSpeed = 0.05f;
                   float duration = Mathf.Clamp(coinsAdd * baseSpeed, 0.2f, 1.2f);

                   DOTween.To(() => oldValue, x =>
                   {
                       oldValue = x;
                       CoinsText.text = x.ToString();
                   }, newValue, duration).SetEase(Ease.OutQuad);

                   CoinsText.transform.DOScale(Vector3.one * 1.3f, 0.1f).OnComplete(() =>
                   {
                       CoinsText.transform.DOScale(Vector3.one, 0.2f);
                       gearBox.SpinWheelOn = false;
                      // gearBox.Resetpos();
                   });
               })
               .AppendInterval(0.1f)
               .AppendCallback(() =>
               {
                   // Reset position and alpha
                   DriftTempScore.transform.position = originalPos;
                   DriftTempScore.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
               });
        }



        [Button]
        public void IncreaseCount()
        { 
            IncreaseCoins(1000);
        }

        [Button]
        public void IncreaseDriftScore()
        {
            IncreaseCoinDrift(1500);
        
        }

        #endregion


        #region Game_Status_Method

        [SerializeField] GameObject GameoverUI;
        [SerializeField] Button GameOverRstBtn;

        public void OpenGameoverUI()
        { 
          RestartBtn.gameObject.SetActive(false);
          GameoverUI.transform.DOScale(Vector3.one,0.25f).SetEase(Ease.OutQuad);
        }


        #endregion

    }

}