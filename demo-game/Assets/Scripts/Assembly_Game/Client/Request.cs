//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using Packet;
//using Shiko.Context;
//using TMPro;

//public class Request : MonoBehaviour {
//    public string PlayerID;
//    public string Token;

//    public ShikoClient shikoClient;
//    public Button buttonAuth, buttonState, buttonStart, buttonDiscardCard, buttonPlayCard, buttonHistory, buttonResetGame, buttonPing;
//    public TextMeshProUGUI handCards, handType, discardCount, discarded, playerGold, cost, winPoint, isAuth;

//    private string _handCards, _handType, _discardCount, _discarded, _playerGold, _cost, _winPoint;
//    private bool _auth = false;

//    private void Awake() {
//        // Register callbacks
//        shikoClient.OnConnect(() => Debug.Log("Successfully connected to the server!"));
//        shikoClient.OnDisconnect(() => Debug.Log("Disconnected from the server!"));
//        shikoClient.OnClose(() => Debug.Log("Connection closed!"));
//        shikoClient.ConnectToServer(true);
//    }

//    // Start is called before the first frame update
//    void Start() {
//        ResetGame();

//        // Assign button click listeners
//        buttonAuth.onClick.AddListener(OnButtonAuthClicked);
//        buttonState.onClick.AddListener(OnButtonStateClicked);
//        buttonStart.onClick.AddListener(OnButtonStartClicked);
//        buttonDiscardCard.onClick.AddListener(OnButtonDiscardCardClicked);
//        buttonPlayCard.onClick.AddListener(OnButtonPlayCardClicked);
//        buttonHistory.onClick.AddListener(OnButtonHistoryClicked);
//        buttonPing.onClick.AddListener(OnButtonPingClicked);

//        buttonResetGame.onClick.AddListener(OnButtonResetGameClicked);
//    }

//    // Update is called once per frame
//    void Update() {
//        handCards.text = _handCards;
//        handType.text = _handType;
//        discardCount.text = _discardCount;
//        discarded.text = _discarded;
//        playerGold.text = _playerGold;
//        cost.text = _cost;
//        winPoint.text = _winPoint;
//        isAuth.text = $"IsAuth: {_auth}";
//    }

//    private void ResetGame() {
//        _handCards = "HandCards: None";
//        _handType = "HandType: None";
//        _discardCount = "Discarded: 0";
//        _discarded = "Discarded Cards: None";
//        _playerGold = "PlayerGold: 0";
//        _cost = "Cost: 0";
//        _winPoint = "Win Points: 0";
//    }

//    // Button click handlers
//    private void OnButtonAuthClicked() {
//        AuthRequest request = new AuthRequest { token = Token };
//        shikoClient.OnRequest("Game.Auth", request, OnAuthResponse);
//    }

//    private void OnButtonStateClicked() {
//        StateRequest request = new StateRequest();
//        shikoClient.OnRequest("Game.State", request, OnStateResponse);
//    }

//    private void OnButtonStartClicked() {
//        StartRequest request = new StartRequest { bet_id = 1 };
//        shikoClient.OnRequest("Game.Start", request, OnStartResponse);
//    }

//    private void OnButtonDiscardCardClicked() {
//        DiscardCardRequest request = new DiscardCardRequest { card_idxs = new List<int> { 0, 1, 2, 3, 4, 5, 6 } };
//        shikoClient.OnRequest("Game.DiscardCard", request, OnDiscardCardResponse);
//    }

//    private void OnButtonPlayCardClicked() {
//        PlayCardRequest request = new PlayCardRequest();
//        shikoClient.OnRequest("Game.PlayCard", request, OnPlayCardResponse);
//    }

//    private void OnButtonHistoryClicked() {
//        HistoryRequest request = new HistoryRequest { n = 100 };
//        shikoClient.OnRequest("Game.History", request, OnHistoryResponse);
//    }

//    private void OnButtonResetGameClicked() {
//        ResetGame();
//    }

//    private void OnButtonPingClicked() {
//        PingRequest request = new PingRequest();
//        shikoClient.OnRequest("Health.Ping", request, OnPingResponse);
//    }

//    // Response handlers
//    private void OnStartResponse(Context context) {
//        StartResponse response = context.Struct<StartResponse>();
//        if (response.error_code != null) {
//            Debug.Log(response.error_code);
//            return;
//        }

//        LogGameState(response.game_state);
//        _playerGold = $"PlayerGold: {response.player_gold}";
//        _cost = $"Cost: {response.cost}";
//    }

//    private void OnStateResponse(Context context) {
//        StateResponse response = context.Struct<StateResponse>();
//        if (response.error_code != null) {
//            Debug.Log(response.error_code);
//            return;
//        }

//        LogGameState(response.game_state);
//        _playerGold = $"PlayerGold: {response.player_gold}";
//    }

//    private void OnAuthResponse(Context context) {
//        AuthResponse response = context.Struct<AuthResponse>();
//        if (response.error_code != null) {
//            Debug.Log(response.error_code);
//            return;
//        }

//        _auth = response.is_auth;
//    }

//    private void OnDiscardCardResponse(Context context) {
//        DiscardCardResponse response = context.Struct<DiscardCardResponse>();
//        if (response.error_code != null) {
//            Debug.Log(response.error_code);
//            return;
//        }

//        LogGameState(response.game_state);
//        _playerGold = $"PlayerGold: {response.player_gold}";
//        _cost = $"Cost: {response.cost}";
//    }

//    private void OnPlayCardResponse(Context context) {
//        PlayCardResponse response = context.Struct<PlayCardResponse>();
//        if (response.error_code != null) {
//            Debug.Log(response.error_code);
//            return;
//        }

//        LogGameState(response.game_state);
//        _playerGold = $"PlayerGold: {response.player_gold}";
//        _winPoint = $"Win Gold: {response.win_gold}";
//    }

//    private void OnHistoryResponse(Context context) {
//        HistoryResponse response = context.Struct<HistoryResponse>();
//        if (response.error_code != null) {
//            Debug.Log(response.error_code);
//            return;
//        }

//        LogHistoryBest(response.history_best);
//        LogGameLogs(response.game_logs);
//    }

//    private void OnPingResponse(Context context) {
//        Debug.Log("Pong");
//    }

//    private void LogGameState(GameState gameState) {
//        if (gameState == null) {
//            Debug.Log("Game state is null.");
//            return;
//        }

//        // Bind data to UI Text elements
//        _handCards = $"Hand Cards: {(gameState.hand_cards != null ? string.Join(", ", gameState.hand_cards) : "No hand cards available.")}";
//        _discardCount = $"Discard Count: {gameState.discard_count}";
//        _handType = $"Current Hand Type: {gameState.curr_hand_type}";

//        // Bind discard cards (2D List) to UI
//        if (gameState.discard_cards != null && gameState.discard_cards.Count > 0) {
//            List<string> discardSets = new List<string>();
//            for (int i = 0; i < gameState.discard_cards.Count; i++)
//                discardSets.Add($"Discard Set {i + 1}: " + string.Join(", ", gameState.discard_cards[i]));

//            // Multi-line for each discard set
//            _discarded = string.Join("\n", discardSets);
//        } else
//            _discarded = "No discard cards available.";
//    }

//    private void LogHistoryBest(GameLog historyBest) {
//        if (historyBest == null) {
//            Debug.Log("History best is null.");
//            return;
//        }

//        string logMessage = $"History best:\n" +
//                                $"Game ID: {historyBest.game_id}\n" +
//                                $"Hand Type: {historyBest.hand_type}\n" +
//                                $"Show Hands: {string.Join(", ", historyBest.show_hands)}\n" +
//                                $"Reward: {historyBest.reward}\n" +
//                                $"Cost: {historyBest.cost}\n" +
//                                $"TotalCost: {historyBest.total_cost}\n" +
//                                $"Swap Counts: {historyBest.swap_counts}\n" +
//                                $"Profit: {historyBest.profit}\n" +
//                                $"Started At: {historyBest.started_at}\n" +
//                                $"Ended At: {historyBest.ended_at}\n" +
//                                "------";

//        Debug.Log(logMessage);
//    }

//    private void LogGameLogs(List<GameLog> gameLogs) {
//        foreach (var gameLog in gameLogs) {
//            string logMessage = $"Game Log:\n" +
//                                $"Game ID: {gameLog.game_id}\n" +
//                                $"Hand Type: {gameLog.hand_type}\n" +
//                                $"Show Hands: {string.Join(", ", gameLog.show_hands)}\n" +
//                                $"Reward: {gameLog.reward}\n" +
//                                $"Cost: {gameLog.cost}\n" +
//                                $"TotalCost: {gameLog.total_cost}\n" +
//                                $"Swap Counts: {gameLog.swap_counts}\n" +
//                                $"Profit: {gameLog.profit}\n" +
//                                $"Started At: {gameLog.started_at}\n" +
//                                $"Ended At: {gameLog.ended_at}\n" +
//                                "------";

//            Debug.Log(logMessage);
//        }
//    }
//}
