using UnityEngine;
using OscSimpl;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Theme;
using Tool;


namespace OSC
{
    public class OSC_Manager : MonoBehaviour
    {
        public static OSC_Manager Instance { get; private set; }

        //receiver
        public OscIn _oscIn;
        //sender
        public OscOut _oscOut;

        [SerializeField] private int portIn = 7005;
        [SerializeField] private int portOut = 8000;

        private string currentSceneName;

        //pour le projet
        const string remoteAccueilTous = "/remote/AccueilTous";     //argument 1
        const string remoteQuit = "/remote/Quit";                 //argument 1
        const string remoteResetScoreBoard = "/remote/ResetAllScore";  //pour les jeux
        const string lesImpacts = "/point";                     //list de float
        const string remoteLaunch = "/remote/Launch";         //argument nom du jeu
        const string remoteStart = "/remote/Start";         //argument nom du jeu
        const string remoteInstruction = "/remote/Instruction"; //argument nom du jeu
        const string remoteAccueil = "/remote/Accueil";     //argument nom du jeu
        const string remoteFct1 = "/remote/PageUp";                 //argument nom du jeu
        const string remoteFct2 = "/remote/PageDown";                 //argument nom du jeu
        const string remoteNameGamer = "/remote/nameGamer";                 //argument nom du joueur
        const string remoteCalibrage = "/remote/Calibrage";                 //argument nom du joueur
        const string remoteVelo = "/remote/Velo";                           //argument intervalle p�dale et angle du guidon*
        const string remoteAthleChoix = "/remote/athleChoix";               //argument r juste pour un affichage sp�cial pour Olympics
        const string remoteStartAthle = "/remote/startAthle";               //argument int (100m or 400m) int (F or M) int (2 or 3 player) int (Choice Country 1) int (Choice Country 2) int (Choice Country 3)
        const string remoteJoyeux = "/remote/Joyeux";                 //argument 1 = photo, 2 = Gateau, 3 = Cadeaux, 4 = Pinata
        const string remoteEcranSelection = "/remote/EcranSelection"; //argument 0 = normal, 1 = Load Ecran Selection

        //pour communiquer avec l'appli Interface
        const string nomJoueur = "/remote/Name";             //argument 1
        //Envoyer lorsqu'une GameScene est lanc�e
        const string accueilAppli = "/remote/Accueil";      //argument 1
        const string enCours = "/remote/Encours";           //argument nom du jeu
        const string remoteQuitAll = "/remote/Quit";         //argument 1
        const string photoMonstresDemo = "/remote/PhotoMonstresDemo";   //argument 1
        const string photoBlockQuestion = "/remote/PhotoblockQuestion"; //argument 1
        const string hide = "/remote/Hide";                             //arguement 1
        const string show = "/remote/Show";                             //argument 1
        const string startChoix = "/remote/startChoix";                 //argument 1
        const string unityReady = "/remote/UnityReady";
        
        const string langue = "/remote/Langue";     //argument 1
        const string tousMessages = "/remote/*";

        const string remotesLangue = "/remote/Langue";
        private const int _nbreOfCharacter = 25;
        OscMessage _message;

        [HideInInspector] public string playerOneName, playerTwoName;
        public delegate void OnNameEnter(string test);
        public OnNameEnter nameDelegate;

        //Mandatory for Monstres game
        public List<Sprite> playersSprites;

        //Mandatory for Photoblock game
        public string chosenPicture;

        public ReceiveParent receiveP { get; set; }
        public RailShooter.RailShooter_GameManager veloReceiver { get; set; }

        public bool inOpened;
        public bool outOpened;

        public List<string> _multipleGameSceneGames = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            // Ensure that we have a OscIn component and start receiving on port 7000.
            if (!_oscIn) _oscIn = gameObject.AddComponent<OscIn>();
            inOpened = _oscIn.Open(portIn);      //TODO : faire que ce soit une variable

            // Ensure that we have a OscOut component.
            if (!_oscOut) _oscOut = gameObject.AddComponent<OscOut>();

            // Prepare for sending messages locally on this device on port 7000.
            outOpened = _oscOut.Open(portOut);     //TODO : faire que ce soit une variable

            UnityReady();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                messageOutQuit();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                SendGamerName("Jean Christophe");
                //SendGamerName("mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm                                        Jean Christophe  Jean Christophe");
            }
        }

        void OnEnable()
        {
            //mapping des messages
            _oscIn.MapInt(remoteAccueilTous, onOSCAccueilTous);
            _oscIn.MapInt(remoteQuit, onOSCQuit);
            _oscIn.MapInt(remoteCalibrage, onOSCCalibrage);

            _oscIn.Map(remoteVelo, onOSCVelo);
            _oscIn.Map(lesImpacts, onOSCPoint);
            _oscIn.Map(remoteLaunch, onOSCLaunch);
            _oscIn.Map(remoteStart, onOSCStart);
            _oscIn.Map(remoteInstruction, onOSCInstruction);
            _oscIn.Map(remoteAccueil, onOSCAccueil);
            _oscIn.Map(remoteFct1, onOSCFct1);
            _oscIn.Map(remoteFct2, onOSCFct2);
            _oscIn.Map(remoteNameGamer, onOSCNameGamer);
            _oscIn.Map(remoteStartAthle, OnStartAthle);
            _oscIn.Map(remoteAthleChoix, OnAthleChoix);
            _oscIn.Map(remoteJoyeux, OnJoyeux);
            _oscIn.Map(remoteEcranSelection, OnEcranSelection);

            //Show the player score
            _oscIn.Map(remoteNameGamer, onShowScore);

            //ScoreBoard Reset All
            _oscIn.Map(remoteResetScoreBoard, OnResetAllScoreBoard);
            _oscIn.Map(tousMessages, OntousMessages);
            _oscIn.Map(langue,OnLanguage);
        }

        void OnDisable()
        {
            // If you want to stop receiving messages you have to "unmap".
            _oscIn.UnmapInt(onOSCAccueilTous);
            _oscIn.UnmapInt(onOSCQuit);
            _oscIn.UnmapInt(onOSCCalibrage);

            _oscIn.Unmap(onOSCVelo);
            _oscIn.Unmap(onOSCPoint);
            _oscIn.Unmap(onOSCLaunch);
            _oscIn.Unmap(onOSCStart);
            _oscIn.Unmap(onOSCInstruction);
            _oscIn.Unmap(onOSCAccueil);
            _oscIn.Unmap(onOSCFct1);
            _oscIn.Unmap(onOSCFct2);
            _oscIn.Unmap(onOSCNameGamer);
            _oscIn.Unmap(onShowScore);
            _oscIn.Unmap(OnStartAthle);
            _oscIn.Unmap(OnAthleChoix);
            _oscIn.Unmap(OnJoyeux);
            _oscIn.Unmap(OntousMessages);

            //ScoreBoard Reset All
            _oscIn.Unmap(OnResetAllScoreBoard);
            _oscIn.Unmap(OnLanguage);
        }

        ///// <summary>
        ///// Pour VVS
        ///// </summary>
        ///// <param name="message"></param>
        //private void onOSCVelo(OscMessage message)
        //{
        //    int intervale;
        //    message.TryGet(0, out intervale);
        //    int angle;
        //    message.TryGet(1, out angle);

        //    if(veloReceiver != null)
        //    {
        //        veloReceiver.ReceiveVeloInfo(intervale, angle);
        //    }

        //    OscPool.Recycle(message);
        //}

        /// <summary>
        /// Pour Velo Cr�teil
        /// </summary>
        /// <param name="message"></param>
        private void onOSCVelo(OscMessage message)
        {

           


            int v1;
            message.TryGet(0, out v1);
            int v2;
            message.TryGet(1, out v2);


            OscPool.Recycle(message);
        }

        //RECEPTION
        public void onOSCAccueilTous(int value)
        {
            _message = new OscMessage(remoteAccueilTous);
            _message.Set(0, 1);
            _oscOut.Send(_message);
            //SceneManager.LoadScene(0);
            SceneTransitionUtility.CleanRAMAndLoadScene(this, 0);
        }

        public void onOSCQuit(int value)
        {
            Application.Quit();
        }

        public void onOSCCalibrage(int value)
        {


            SceneManager.LoadScene("Calibrage");
        }
        
        public void OntousMessages(OscMessage message)
        {
            Debug.Log("Tous les shortcuts désactivés avec remote/*");
            Universal_GeneralVariables.SetShortcutsEnabled(true);
        }

        public void onOSCPoint(OscMessage message)
        {

           

            float impactX;
            message.TryGet(0, out impactX);
            float impactY;
            message.TryGet(1, out impactY);

            var allReceiveP = FindObjectsOfType<ReceiveParent>();
            foreach (var r in allReceiveP)
            {
                r.ReceivePoint(impactX, impactY);
            }

            ////Envoie les valeurs du point � receivePoints de chaque jeu pour g�rer l'impact
            //if (receiveP != null)
            //{
            //    Debug.Log(impactX + " : " + impactY);
            //    receiveP.ReceivePoint(impactX, impactY);
            //}

            // Always recycle incoming messages when used.
            OscPool.Recycle(message);
        }

        public void onOSCLaunch(OscMessage message)
        {


            //ouvrir la scene accueil du jeu (sauf pour Monstres & Photoblock)
            string nomJeu = "";
            //nomJeu = nomJeu.Substring(4);
            message.TryGet(0, ref nomJeu);

            Debug.Log("onOSCLaunch : " + nomJeu);
            SceneManager.LoadScene("Accueil_" + nomJeu);
            currentSceneName = "Accueil_" + nomJeu;

            // Always recycle incoming messages when used.
            OscPool.Recycle(message);
        }

        public void onOSCStart(OscMessage message)
        {


            //ouvrir la scene game du jeu
            string nomJeu = "";
            //nomJeu = nomJeu.Substring(4);
            message.TryGet(0, ref nomJeu);

            if (!_multipleGameSceneGames.Contains(nomJeu))
            {
                currentSceneName = "GameScene_" + nomJeu;
                Debug.Log("Game Load OSC : " + currentSceneName);
                LoadingManager.LoadScene("GameScene_" + nomJeu);
            }

            // Always recycle incoming messages when used.
            OscPool.Recycle(message);
        }

        public void onOSCInstruction(OscMessage message)
        {

            //ouvrir la scene game du jeu
            string nomJeu = "";
            //nomJeu = nomJeu.Substring(4);
            message.TryGet(0, ref nomJeu);

            SceneManager.LoadScene("Intro_" + nomJeu);
            currentSceneName = "Intro_" + nomJeu;

            // Always recycle incoming messages when used.
            OscPool.Recycle(message);
        }

        public void onOSCAccueil(OscMessage message)
        {

            //ouvrir la scene accueil du jeu
            string nomJeu = "";
            //nomJeu = nomJeu.Substring(4);
            message.TryGet(0, ref nomJeu);

            Debug.Log("onOSCAcceuil : Accueil_" + nomJeu);
            currentSceneName = "Accueil_" + nomJeu;
            currentSceneName = ToolBox.GetGameNameWithoutSuffix(currentSceneName);

            //SceneManager.LoadScene(currentSceneName);
            SceneTransitionUtility.CleanRAMAndLoadScene(this, currentSceneName);
            //onOSCAccueilAppli();
            // Always recycle incoming messages when used.
            OscPool.Recycle(message);
        }

        public void onOSCFct1(OscMessage message)
        {

            //dans la scene score on fait defiler vers le haut
            string nomJeu = "";
            //nomJeu = nomJeu.Substring(4);
            message.TryGet(0, ref nomJeu);

            FindObjectOfType<LeaderboardParent>()?.ScrollUp();
            FindObjectOfType<ScoreBoardDisplayer>()?.PageUp();

            // Always recycle incoming messages when used.
            OscPool.Recycle(message);
        }

        public void onOSCFct2(OscMessage message)
        {

            //dans la scene score on fait defiler vers le bas
            string nomJeu = "";
            //nomJeu = nomJeu.Substring(4);
            message.TryGet(0, ref nomJeu);

            FindObjectOfType<LeaderboardParent>()?.ScrollDown();
            FindObjectOfType<ScoreBoardDisplayer>()?.PageDown();

            // Always recycle incoming messages when used.
            OscPool.Recycle(message);
        }

        public void OnResetAllScoreBoard(OscMessage message)
        {
           
            Debug.Log("Reset All Scoreboard");
            ScoreBoardManager.ResetAll();
            OldScoreBoardManager.ResetAll();
        }

        public void OnStartAthle(OscMessage message)
        {
           
            Universal_GeneralVariables universal_GeneralVariables = FindObjectOfType<Universal_GeneralVariables>();
            if (universal_GeneralVariables != null)
            {
                universal_GeneralVariables.OnConfigGame(message);
            }
            OscPool.Recycle(message);
        }

        public void OnAthleChoix(OscMessage message)
        {
           
            Universal_GeneralVariables universal_GeneralVariables = FindObjectOfType<Universal_GeneralVariables>();
            if (universal_GeneralVariables != null)
            {
                universal_GeneralVariables.OnChoix(message);
            }
            OscPool.Recycle(message);
        }

        public void OnJoyeux(OscMessage message)
        {       
           
            Universal_GeneralVariables universal_GeneralVariables = FindObjectOfType<Universal_GeneralVariables>();
            if (universal_GeneralVariables != null)
            {
                universal_GeneralVariables.OnChoix(message);
            }
            OscPool.Recycle(message);
        }

        public void OnEcranSelection(OscMessage message)
        {
           
            int loadMenuSelection = 0;
            if (message.TryGet(0, out loadMenuSelection))
            {
                BuildState.CurrentState = (BuildState.State)loadMenuSelection;
                if (SceneManager.GetActiveScene().name != "Boot")
                {
                    SceneManager.LoadScene(loadMenuSelection == 0 ? "AccueilTous" : BuildState.MenuSelectionSceneName);
                }
            }
            OscPool.Recycle(message);
        }

        public void onShowScore(OscMessage message)
        {
           
            string nomJoueur = "";
            message.TryGet(0, ref nomJoueur);
            //SendGamerName(nomJoueur);


            OscPool.Recycle(message);
        }

        public void onOSCNameGamer(OscMessage message)
        {
            Universal_GeneralVariables.SetShortcutsEnabled(false);
            //on renseigne le nom du joueur pour le tableau des scores
            string nomGamer = "";

            message.TryGet(0, ref nomGamer);
            SendGamerName(nomGamer);
            OscPool.Recycle(message);
        }

        public void OnLanguage(OscMessage message)
        {
            int index;

            if (message.TryGet(0, out index))
            {
                // On vérifie si l'index correspond à une valeur existante dans l'Enum
                if (System.Enum.IsDefined(typeof(Language), index))
                {
                    Localizer.currentLanguage = (Language)index;
                    UnityEngine.Debug.Log($"Langue changée : {Localizer.currentLanguage}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Index de langue reçu ({index}) n'existe pas dans l'Enum Language.");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Le message OSC ne contient pas d'entier à l'index 0.");
            }

            OscPool.Recycle(message);
        }
        

        private void SendGamerName(string nomGamer)
        {

            Universal_GeneralVariables universal_GeneralVariables = FindObjectOfType<Universal_GeneralVariables>();
            Universal_GeneralVariables.SetShortcutsEnabled(false);
            
            nomGamer = nomGamer.RemoveDiacritics();
            nomGamer = nomGamer.RemoveSpecialCharacters();
            nomGamer = Regex.Replace(nomGamer, @"\s+", " ");
            nomGamer = nomGamer.Substring(0, Mathf.Min(nomGamer.Length, _nbreOfCharacter));
            LeaderboardParent leaderboardParent = FindObjectOfType<LeaderboardParent>();
            if (leaderboardParent != null)
            {
                leaderboardParent.NameSubmit(nomGamer);
            }

            if (universal_GeneralVariables != null)
            {
                Debug.Log("[OSc] SendGamerName");
                universal_GeneralVariables.ReceiveName(nomGamer);
            }
            else
            {
                Debug.Log("[OSc] SenGamerNamerFailure");
            }

            
            Universal_GeneralVariables.SetShortcutsEnabled(true);
        }

        //ENVOI
        public void messageOutQuit()
        {
            
            _message = new OscMessage(remoteQuitAll);
            _message.Set(0, 1);
            _oscOut.Send(_message);
            Application.Quit();
        }

        public void GameEnCours()
        {
            _message = new OscMessage(enCours);
            _message.Set(0, 1);
            _oscOut.Send(_message);
        }

        public void ShowSoftKeyboard()
        {
            Universal_GeneralVariables.SetShortcutsEnabled(false);
            _message = new OscMessage(nomJoueur);
            _message.Set(0, 1);
            _oscOut.Send(_message);
        }

        public void PhotoMonstresDemo()
        {
            _message = new OscMessage(photoMonstresDemo);
            _message.Set(0, 1);
            _oscOut.Send(_message);
        }

        public void PhotoBlockQuestion()
        {
            _message = new OscMessage(photoBlockQuestion);
            _message.Set(0, 1);
            _oscOut.Send(_message);
        }

        public void onOSCAccueilAppli()
        {
            _message = new OscMessage(accueilAppli);
            _message.Set(0, 1);
            _oscOut.Send(_message);
        }

        public void Hide()
        {
            _message = new OscMessage(hide);
            _message.Set(0, 1);
            _oscOut.Send(_message);
        }

        public void Show()
        {
            _message = new OscMessage(show);
            _message.Set(0, 1);
            _oscOut.Send(_message);
        }

        public void StartChoix()
        {
            _message = new OscMessage(startChoix);
            _message.Set(0, 1);
            _oscOut.Send(_message);
        }

        public void UnityReady()
        {
            _message = new OscMessage(unityReady);
            _message.Set(0, 1);
            _oscOut.Send(_message);
            //Debug.Log("UnityReady");
        }

        public void SendAccueilTous()
        {
            _message = new OscMessage(remoteAccueilTous);
            _message.Set(0, 1);
            _oscOut.Send(_message);
        }

        public void DeactivateAllOscMessages()
        {
            //IsOscActivated = false;

            //_oscIn.UnmapInt(onOSCAccueilTous);
            //_oscIn.UnmapInt(onOSCQuit);
            //_oscIn.UnmapInt(onOSCCalibrage);

            //_oscIn.Unmap(onOSCVelo);
            //_oscIn.Unmap(onOSCPoint);
            //_oscIn.Unmap(onOSCLaunch);
            //_oscIn.Unmap(onOSCStart);
            //_oscIn.Unmap(onOSCInstruction);
            //_oscIn.Unmap(onOSCAccueil);
            //_oscIn.Unmap(onOSCFct1);
            //_oscIn.Unmap(onOSCFct2);
            //_oscIn.Unmap(onOSCNameGamer);
            //_oscIn.Unmap(onShowScore);
            //_oscIn.Unmap(OnStartAthle);
            //_oscIn.Unmap(OnAthleChoix);
            //_oscIn.Unmap(OnJoyeux);
            //_oscIn.Unmap(OnEcranSelection);
            //_oscIn.Unmap(OnResetAllScoreBoard);

            this.enabled = false;
            Debug.Log("OSC messages deactivated");
        }

        public void ReactivateAllOscMessages()
        {
            //IsOscActivated = true;

            //_oscIn.MapInt(remoteAccueilTous, onOSCAccueilTous);
            //_oscIn.MapInt(remoteQuit, onOSCQuit);
            //_oscIn.MapInt(remoteCalibrage, onOSCCalibrage);

            //_oscIn.Map(remoteVelo, onOSCVelo);
            //_oscIn.Map(lesImpacts, onOSCPoint);
            //_oscIn.Map(remoteLaunch, onOSCLaunch);
            //_oscIn.Map(remoteStart, onOSCStart);
            //_oscIn.Map(remoteInstruction, onOSCInstruction);
            //_oscIn.Map(remoteAccueil, onOSCAccueil);
            //_oscIn.Map(remoteFct1, onOSCFct1);
            //_oscIn.Map(remoteFct2, onOSCFct2);
            //_oscIn.Map(remoteNameGamer, onOSCNameGamer);
            //_oscIn.Map(remoteStartAthle, OnStartAthle);
            //_oscIn.Map(remoteAthleChoix, OnAthleChoix);
            //_oscIn.Map(remoteJoyeux, OnJoyeux);
            //_oscIn.Map(remoteEcranSelection, OnEcranSelection);

            //_oscIn.Map(remoteNameGamer, onShowScore);
            //_oscIn.Map(remoteResetScoreBoard, OnResetAllScoreBoard);


            this.enabled = true;
            Debug.Log("OSC messages reactivated");
        }


    }
}
