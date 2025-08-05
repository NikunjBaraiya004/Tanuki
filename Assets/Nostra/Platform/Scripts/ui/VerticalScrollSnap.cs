using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace nostra.core.ui
{
    public class VerticalScrollSnap : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Scroll Settings")]
        [SerializeField] private float snapSpeed = 2f;
        [SerializeField] private float minFlickVelocity = 1000f; // Minimum velocity to trigger scroll
        [SerializeField] private Transform contentTransform;
        [SerializeField] private int paginationTriggerOffset = 2;

        private const int CARD_COUNT = 5;
        private const int BUFFER_COUNT = 2;

        private Vector2 lastDragPosition;
        private float lastDragTime;
        private Vector2 dragVelocity;
        private Vector3 currentVelocity;
        private bool isDragging;
        private bool isScrolling;
        private bool isLoadingMore;
        private bool hasMorePosts = true;
        private float screenHeight;
        private int lastSnappedIndex = -1;
        private Vector2 dragStartPosition;

        // Core state
        private int totalPosts;
        private int currentCardIndex;
        private int currentIndex;
        private int scrollCount;
        private readonly RectTransform[] cards = new RectTransform[CARD_COUNT];
        private readonly int[] cardToPostIndex = new int[CARD_COUNT];
        private readonly int[] cardToCardIndex = new int[CARD_COUNT];

        // Events
        public event Action<int, int> OnCardChanged;     // (postIndex, cardIndex)
        public event Action<int> OnScrollStart;          // currentIndex
        public event Action<int> OnScrollEnd;            // finalIndex
        public event Action OnLoadMorePosts;

        private void Awake()
        {
            screenHeight = 1600;
        }

        public void Initialize(int postCount, bool hasMore = true)
        {
            totalPosts = postCount;
            hasMorePosts = hasMore;
            isLoadingMore = false;
            currentIndex = 0;
            scrollCount = 0;
            contentTransform.localPosition = Vector3.zero;
            SetupCards();
        }

        private void SetupCards()
        {
            if (contentTransform.childCount < CARD_COUNT) return;

            // Position cards from top to bottom (0 to -4 * screenHeight)
            int maxCards = Mathf.Min(CARD_COUNT, totalPosts);
            for (int i = 0; i < CARD_COUNT; i++)
            {
                cards[i] = contentTransform.GetChild(i).GetComponent<RectTransform>();
                cards[i].anchoredPosition = new Vector2(0, -i * screenHeight);
                cardToPostIndex[i] = i;
                cardToCardIndex[i] = i;
                if (i >= maxCards)
                {
                    cards[i].gameObject.SetActive(false);
                }
                else
                {
                    cards[i].gameObject.SetActive(true);
                    OnCardChanged?.Invoke(i, i);
                }
            }

            contentTransform.localPosition = new Vector2(0, 0);
            currentCardIndex = 0;
        }

        private void UpdateCardPositions()
        {
            int minBottomCard = Mathf.Min(2, totalPosts - currentIndex - 1);
            int currentBottomCard = 0;
            for (int i = 0; i < CARD_COUNT; i++)
            {
                if (cards[i].anchoredPosition.y < cards[currentCardIndex].anchoredPosition.y)
                {
                    currentBottomCard++;
                }
            }
            for (int i = 0; i < (minBottomCard - currentBottomCard); i++)
            {
                RecycleTopToBottom();
            }
        }

        private void RecycleTopToBottom()
        {
            // Move top card to bottom
            var topCard = cards[0];
            int newPostIndex = cardToPostIndex[0] + CARD_COUNT;
            int nextCardIndex = cardToCardIndex[0];

            if (newPostIndex < totalPosts)
            {
                // Shift array up
                for (int i = 0; i < CARD_COUNT - 1; i++)
                {
                    cards[i] = cards[i + 1];
                    cardToPostIndex[i] = cardToPostIndex[i + 1];
                    cardToCardIndex[i] = cardToCardIndex[i + 1];
                }

                cards[CARD_COUNT - 1] = topCard;
                cardToPostIndex[CARD_COUNT - 1] = newPostIndex;
                cardToCardIndex[CARD_COUNT - 1] = nextCardIndex;
                // Update position and contentsc
                topCard.anchoredPosition = new Vector2(0, topCard.anchoredPosition.y - (CARD_COUNT * screenHeight));
                OnCardChanged?.Invoke(nextCardIndex, newPostIndex);
            }
        }
        private void RecycleBottomToTop()
        {
            // Move bottom card to top
            var bottomCard = cards[CARD_COUNT - 1];
            int newPostIndex = cardToPostIndex[CARD_COUNT - 1] - CARD_COUNT;
            int nextCardIndex = cardToCardIndex[CARD_COUNT - 1];

            if (newPostIndex >= 0)
            {
                // Shift array down
                for (int i = CARD_COUNT - 1; i > 0; i--)
                {
                    cards[i] = cards[i - 1];
                    cardToPostIndex[i] = cardToPostIndex[i - 1];
                    cardToCardIndex[i] = cardToCardIndex[i - 1];
                }

                cards[0] = bottomCard;
                cardToPostIndex[0] = newPostIndex;
                cardToCardIndex[0] = nextCardIndex;
                // Update position and content
                bottomCard.anchoredPosition = new Vector2(0, bottomCard.anchoredPosition.y + (CARD_COUNT * screenHeight));
                OnCardChanged?.Invoke(nextCardIndex, newPostIndex);
            }
        }

        private void Update()
        {
            if (!isDragging)
            {
                UpdateScrollPosition();
            }
        }

        private void UpdateScrollPosition()
        {
            float targetY = currentIndex * screenHeight;
            Vector3 targetPosition = new Vector3(0, targetY, 0);
            Vector3 currentPosition = contentTransform.localPosition;

            contentTransform.localPosition = Vector3.SmoothDamp(
                currentPosition,
                targetPosition,
                ref currentVelocity,
                1f / snapSpeed
            );

            // Check if we've essentially reached the target position
            if (Vector3.Distance(contentTransform.localPosition, targetPosition) < 1f)
            {
                contentTransform.localPosition = targetPosition;
                if (lastSnappedIndex != currentCardIndex)
                {
                    OnScrollEnd?.Invoke(currentCardIndex);
                    lastSnappedIndex = currentCardIndex;
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            dragStartPosition = eventData.position;
            lastDragPosition = eventData.position;
            lastDragTime = Time.time;
            dragVelocity = Vector2.zero;
            OnScrollStart?.Invoke(currentCardIndex);
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            Vector2 currentPosition = eventData.position;
            float currentTime = Time.time;
            float deltaTime = currentTime - lastDragTime;
            
            if (deltaTime > 0)
            {
                Vector2 deltaPosition = currentPosition - lastDragPosition;
                dragVelocity = deltaPosition / deltaTime; // Calculate velocity in pixels per second
            }
            
            lastDragPosition = currentPosition;
            lastDragTime = currentTime;

            // Move content with drag
            Vector3 newPosition = contentTransform.localPosition + new Vector3(0, eventData.delta.y, 0);
            contentTransform.localPosition = newPosition;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            Vector2 currentPosition = eventData.position;
            float currentTime = Time.time;
            float deltaTime = currentTime - lastDragTime;
            if (deltaTime > 0)
            {
                Vector2 deltaPosition = currentPosition - lastDragPosition;
                dragVelocity = deltaPosition / deltaTime; // Calculate velocity in pixels per second
            }
            lastDragPosition = currentPosition;
            lastDragTime = currentTime;
            isDragging = false;
            float velocityY = dragVelocity.y;

            if (Mathf.Abs(velocityY) > minFlickVelocity || Vector2.Distance(dragStartPosition, eventData.position) > screenHeight * 0.5f)
            {
                // Natural scrolling: flicking up (positive velocity) shows next content
                bool scrollingUp = velocityY > 0;
                if (scrollingUp && currentIndex < totalPosts - 1)
                {
                    currentIndex++;
                    scrollCount++;
                    currentCardIndex++;
                    if (currentCardIndex >= CARD_COUNT)
                    {
                        currentCardIndex = 0;
                    }
                    // Recycle during second scroll
                    if (scrollCount > 2)
                    {
                        RecycleTopToBottom();
                    }
                }
                else if (!scrollingUp && currentIndex > 0)
                {
                    currentIndex--;
                    scrollCount--;
                    currentCardIndex--;
                    if (currentCardIndex < 0)
                    {
                        currentCardIndex = CARD_COUNT - 1;
                    }
                    // Recycle during second scroll
                    if (scrollCount < totalPosts - 3)
                    {
                        RecycleBottomToTop();
                    }
                }
            }
            else
            {
                // If velocity is too low, snap back to current position
                // contentTransform.localPosition = new Vector3(0, currentIndex * screenHeight, 0);
            }

            CheckPagination();
        }

        private void CheckPagination()
        {
            if (isLoadingMore || !hasMorePosts) return;

            int triggerIndex = totalPosts - paginationTriggerOffset;
            if (currentIndex >= triggerIndex)
            {
                isLoadingMore = true;
                OnLoadMorePosts?.Invoke();
            }
        }

        public void AddPosts(int newPostCount, bool hasMore = true)
        {
            totalPosts += newPostCount;
            hasMorePosts = hasMore;
            isLoadingMore = false;
            UpdateCardPositions();
        }

        public int GetCurrentIndex() => currentIndex;
        public int GetTotalPosts() => totalPosts;
        public void GoToPost(int targetPostIndex)
        {
            if (targetPostIndex < 0 || targetPostIndex >= totalPosts)
                return;

            lastSnappedIndex = -1;
            int indexDiff = targetPostIndex - currentIndex;
            if (indexDiff == 0) 
            {
                return;
            }
            currentIndex = targetPostIndex;
            currentCardIndex = (currentCardIndex + indexDiff) % CARD_COUNT;
            if (currentCardIndex < 0) currentCardIndex += CARD_COUNT;
            scrollCount += indexDiff;
            // Recycle cards as needed
            if (indexDiff > 0)
            {
                for (int i = 0; i < indexDiff; i++)
                {
                    RecycleTopToBottom();
                }
            }
            else
            {
                for (int i = 0; i < -indexDiff; i++)
                {
                    RecycleBottomToTop();
                }
            }
            // for(int i = 0; i< cardToPostIndex.Length; i++)
            // {
            //     OnCardChanged?.Invoke(i, cardToPostIndex[i]);
            // }

            // Trigger events
            OnScrollStart?.Invoke(currentCardIndex);

            // Check pagination after jump
            CheckPagination();
        }
    }
}