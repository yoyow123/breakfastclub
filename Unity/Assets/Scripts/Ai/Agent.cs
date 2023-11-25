using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Text;
using System.Linq;

public struct InteractionRequest
{
    public Agent source;
    public AgentBehavior action;

    public InteractionRequest(Agent source, AgentBehavior action)
    {
        this.source = source;
        this.action = action;
    }
}

public class Agent : MonoBehaviour
{
    private int ticksOnThisTask;
    public double[] scores;

    [SerializeField] public int seed;
    [SerializeField] public string studentname;

    private GlobalRefs GR;
    private CSVLogger Logger;
    public SimulationConfig SC;
    public SampleConfig sampleConfig;

    [HideInInspector] public Classroom classroom;
    [HideInInspector] public NavMeshAgent navagent;
    [HideInInspector] public double turnCnt = -1;

    public Personality personality { get; protected set; }
    public double conformity;

    public double happiness { get; set; }
    public double motivation { get; set; }
    public double attention { get; protected set; }
    [SerializeField] private int lastMessagesIdx = 0;
    [SerializeField] private string[] lastMessages = new string[5];
    private StringBuilder lastMessageStringBuilder = new StringBuilder();

    //private List<AgentBehavior> behaviors = new List<AgentBehavior>();
    private Dictionary<string, AgentBehavior> behaviors = new Dictionary<string, AgentBehavior>();
    public AgentBehavior currentAction { get; protected set; }
    public AgentBehavior Desire { get; protected set; }
    public AgentBehavior previousAction { get; protected set; }

    public List<AgentBehavior> actionLists = new List<AgentBehavior>();
    public List<String> actionStringLists = new List<String>();

    private Queue pendingInteractions = new Queue();

    public System.Random random;

    public ActionCount actionCount = new ActionCount();

    public string gptName;

    public List<Friend> friendLists = new List<Friend>();



    public void initAgent(string name, System.Random random, Personality personality)
    {
        this.random = random;
        this.personality = personality;
        studentname = name;

        navagent = GetComponent<NavMeshAgent>();
        //navagent.updatePosition = false;
        //navagent.updateRotation = false;


        GR = GlobalRefs.Instance;
        Logger = GR.logger;
        classroom = GR.classroom;
        SC = classroom.simulationConfig;

        // Define all possible actions
        behaviors.Add("Rest", new Rest(this));
        behaviors.Add("Disagreement", new Disagreement(this));
        behaviors.Add("Communication", new Communication(this));
        behaviors.Add("SoloTime", new SoloTime(this));
        behaviors.Add("InteractionTime", new InteractionTime(this));

        // Set the default action state to Break
        currentAction = behaviors["Rest"];
        previousAction = null;
        Desire = behaviors["Rest"];
        scores = new double[behaviors.Count];

        // Initiate Happiness and Motivation
        motivation = random.Next(100) / 100.0; // with a value between [0.0, 1.0]
        happiness = random.Next(100) / 100.0; // with a value between [0.0, 1.0]


        if (SC.Agent["USE_CONFORMITY_MODEL"] > 0.0)
        {
            // Calculate agent conformity
            double stability = (1.0 - personality.neuroticism) * 0.5
                    + personality.agreeableness * 0.6
                    + personality.conscientousness * 0.6;

            double plasticity = personality.extraversion * 0.8 + personality.openess * 0.5;

            conformity = stability * 0.8 - plasticity * 0.45;
        }
        else
        {
            conformity = SC.Agent["CONFORMITY"];
        }
        //personality.extraversion = 0.9f;

        //select tag and append to gpt: 
        SetGptName();
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
    }

    void Start()
    {
        // Export Agent information using the normal logging system
        // Indicate this 'special' info by setting the turncounter to a negative (invalid) value
        turnCnt = -2;
        LogX(String.Format($"{studentname}|{personality.name}|{conformity}||"), "S");
        turnCnt = -1;
        LogX(String.Format($"{personality.openess}|{personality.conscientousness}|{personality.extraversion}|{personality.agreeableness}|{personality.neuroticism}"), "S");

        turnCnt = 0;
        LogInfo("Agent Personality: " + personality);

        //Agents AG = GameObject.Find("Agents").GetComponent<Agents>();
        LogState();
    }

    // MAIN LOGIC : Called at each iteration
    void FixedUpdate()
    {
        turnCnt++;

        LogState();

        SelectAction();

        //record previous activity
        if (actionLists.Count == 0)
        {
            if (currentAction != previousAction && previousAction != null && previousAction.state == AgentBehavior.ActionState.INACTIVE)
            {
                actionLists.Add(previousAction);
            }
        }
        else if (actionLists.Count < 4)
        {
            if (currentAction != previousAction && previousAction != actionLists[actionLists.Count - 1] && previousAction.state == AgentBehavior.ActionState.INACTIVE)
            {
                actionLists.Add(previousAction);
            }
        }
        else
        {
            actionLists.Clear();
        }


        HandleInteractions();

        UpdateHappiness();

        UpdateAttention();


        //GetComponent<Rigidbody>().velocity = navagent.desiredVelocity;


    }

    public override string ToString()
    {
        return $"{gameObject.name} ({studentname})";
    }

    // Add given Agent and Action to event Queue
    public void Interact(Agent source, AgentBehavior action)
    {
        pendingInteractions.Enqueue(new InteractionRequest(source, action));
    }

    // Log message as info
    public void LogError(string message)
    {
        AppendToLastMessage(message);
        LogX(message, "E");
    }

    public void LogInfo(string message)
    {
        LogX(message, "I");
    }

    public void LogDebug(string message)
    {
        AppendToLastMessage(message);
        LogX(message, "D");
    }

    public void LogX(string message, string type)
    {
        string[] msg = { gameObject.name, turnCnt.ToString(), type, message };
        Logger.log(msg);
    }

    private void AppendToLastMessage(string message)
    {
        lastMessages[lastMessagesIdx % lastMessages.Length] = message;
        lastMessagesIdx++;
    }

    public string GetLastMessage()
    {
        lastMessageStringBuilder.Clear();
        for (int i = 0; i < lastMessages.Length; i++) {
            lastMessageStringBuilder.AppendLine(lastMessages[i]);
        }
        return lastMessageStringBuilder.ToString();
    }

    // Helper function logging Agent state
    private void LogState(bool include_info_log = true)
    {
        if (include_info_log)
            LogX(String.Format($"Motivation {motivation} | Happiness {happiness} | Attenion {attention} | Action {currentAction} | Desire {Desire}"), "I");
        LogX(String.Format($"{motivation}|{happiness}|{attention}|{currentAction}|{Desire}"), "S");
    }

    public string GetStatus()
    {
        return String.Format("{0}\nMotivation {1} Happiness {2} Attenion {3}\nAction {4}\nDesire {5}", gameObject.name, motivation, happiness, attention, currentAction, Desire);
    }



    // attention = f(State, Environment, Personality)
    private void UpdateAttention()
    {
        // Attention is zero untill the agent is actively studying!
        attention = 0.0;
        if ((currentAction is SoloTime) || (currentAction is InteractionTime))
        {
            if (currentAction.state == AgentBehavior.ActionState.ACTION)
            {
                attention = AgentBehavior.boundValue(0.0, personality.conscientousness + motivation - classroom.noise * SC.Agent["ATTENTION_NOISE_SCALE"], 1.0);
            }
        }
    }

    // If current_action equals desire we are happy, sad otherwise
    private void UpdateHappiness()
    {
        // Do not touch happiness if in Disagreement because it will be regulated by Action execution
        if (currentAction is Disagreement)
        {
            return;
        }

        double change;
        if (currentAction == Desire)
        {
            //change = HAPPINESS_INCREASE;
            change = SC.Agent["ACTION_ALIGNMENT_HAPPINESS_INCREASE"];
        }
        else
        {
            //change = -HAPPINESS_DECREASE * (1.0 - personality.neuroticism);
            // Not working, because of too low scaler
            //change = -SC.Agent["ACTION_CONFLICT_HAPPINESS_DECREASE"] * AgentBehavior.boundValue(0.0, personality.neuroticism - personality.agreeableness, 1.0);
            change = -SC.Agent["ACTION_CONFLICT_HAPPINESS_DECREASE"] * personality.neuroticism;
        }
        happiness = AgentBehavior.boundValue(0.0, happiness + change, 1.0);
    }

    private void updateActionStringList(AgentBehavior currentAction)
    {
        String curActionString = currentAction.ToString();
        if (actionStringLists.Count > 0)
        {
            if (!curActionString.Equals(actionStringLists[actionStringLists.Count - 1]))
            {
                actionStringLists.Add(curActionString);
            }
        }
        else
        {
            actionStringLists.Add(curActionString);
        }

        if (actionStringLists.Count > 4)
        {
            actionStringLists.RemoveAt(0); // Remove the oldest element
        }
    }

    private bool StartAction(AgentBehavior newAction, bool setDesire = true, bool startAlternativeAction = true)
    {
        if (newAction.possible())
        {
            if (newAction != currentAction)
            {
                LogInfo(String.Format("Ending current action {0}.", currentAction.name));
                currentAction.end();
                previousAction = currentAction;

                LogInfo(String.Format("Starting new action {0}. Executing ...", newAction.name));
                bool success = newAction.execute();
                if (!success)
                    LogDebug(String.Format("Executing new action failed! Will continou anyways! ..."));
                currentAction = newAction;
                ticksOnThisTask = 0;

                // If we start a new action make this the desired action
                if (setDesire)
                {
                    Desire = newAction;
                }

                updateActionStringList(currentAction);
            }
            else
            {
                // Continue to execute the current action
                newAction.execute();
                if (newAction.state == AgentBehavior.ActionState.ACTION)
                    ticksOnThisTask++;

                updateActionStringList(currentAction);
            }
            return true;
        }
        else
        {
            // We want to execute this action, but cannot
            //Desire = newAction;
            ticksOnThisTask = 0;

            if (startAlternativeAction)
            {
                // If we should execute an alternative, Execute the Desired Action
                // If the desired action is the new Action, we have to chose break, because we tested above that we cannot execute the new action!
                if (Desire != newAction)
                {
                    // If we already execute the desired action, just continue
                    LogDebug(String.Format($"{newAction} is not possible. Will executed the desired action {Desire} instead! ..."));
                    if (Desire == currentAction)
                    {
                        LogDebug(String.Format($"We are already executing the desired action! Just stick with it!"));
                        currentAction.execute();
                        updateActionStringList(currentAction);
                        return true;
                    }
                    else
                    {
                        if (Desire.possible())
                        {
                            currentAction.end();
                            previousAction = currentAction;
                            currentAction = Desire;
                            currentAction.execute();
                            updateActionStringList(currentAction);
                            return true;
                        }
                        else
                        {
                            // Its not possible to execute the desired action, execute break instead
                            newAction = Desire;
                        }
                    }
                }
                // Reavulate actions, masking newAction
                int idx = behaviors.Values.ToList().FindIndex(b => b == newAction);
                double[] masked_scores = new double[scores.Length];
                scores.CopyTo(masked_scores, 0);
                // Mask newAction
                masked_scores[idx] = -1;
                AgentBehavior best_action = selectAction(masked_scores);
                // Try to execute this alternative action
                // If it is not possible do break. And dont set desire to this action.
                if (StartAction(best_action, setDesire = false, startAlternativeAction = false))
                {
                    LogDebug($"Could not execute {newAction} instead chose {best_action}! ...");
                    updateActionStringList(best_action);
                    return true;
                }
            }
            // Agent cannot perform Action, or Desire, execute break instead
            LogDebug($"{newAction} is not possible. Executing break instead! ...");
            currentAction.end();
            previousAction = currentAction;
            currentAction = behaviors["Rest"];
            currentAction.execute();
            updateActionStringList(currentAction);
            return false;
        }
    }
    public string GetPreviousActionLists()
    {
        string str = "";
        /*if (actionLists.Count > 0)
        {
            for (int i = 0; i < actionLists.Count; i++)
            {
                str += actionLists[i].name + "(INACTIVE) " + "\n";
            }
        }*/
        if (actionStringLists.Count > 1)
        {
            for (int i = 0; i < actionStringLists.Count - 1; i++)
            {
                str += actionStringLists[i] + "\n";
            }
        }
        return str;
    }


    private void HandleInteractions()
    {
        while (pendingInteractions.Count > 0)
        {
            InteractionRequest iR = (InteractionRequest)pendingInteractions.Dequeue();
            LogDebug(String.Format("Interaction Request from {0} for action {1}", iR.source, iR.action));
            if (iR.action is Communication)
            {
                HandleCommunication(iR.source);
            }
            else if (iR.action is Disagreement)
            {
                HandleDisagreement(iR.source);
            }
        }
    }

    private void HandleCommunication(Agent otherAgent)
    {
        if ((currentAction is Communication) || (Desire is Communication))
        {
            LogDebug(String.Format("Accept invitation to Communication with {0} ...", otherAgent));
            Communication Communication = (Communication)behaviors["Communication"];
            Communication.acceptInviation(otherAgent);
            StartAction(Communication);
        
        }
        else
        {
            // An agent is convinced to Communication based on its conscientousness trait.
            // Agents high on consciousness are more difficult to convince/distract
            float x = random.Next(100) / 100.0f;
            LogDebug(String.Format("Agent proposal {0} >= {1} ...", x, personality.conscientousness));
            if (x >= personality.conscientousness)
            {
                LogDebug(String.Format("Agent got convinced by {0} to start Communicationting ...", otherAgent));
                Communication Communication = (Communication)behaviors["Communication"];
                Communication.acceptInviation(otherAgent);
                StartAction(Communication, false, false);
            }
            else
            {
                LogDebug(String.Format("Agent keeps to current action ({0} < {1})", x, personality.conscientousness));
            }
        }
    }

    private void HandleDisagreement(Agent otherAgent)
    {
        if ((currentAction is Disagreement) || (Desire is Disagreement))
        {
            LogDebug(String.Format("Agent wanted to Disagreement! Now he can do so with {0} ...", otherAgent));
            Disagreement Disagreement = (Disagreement)behaviors["Disagreement"];
            Disagreement.acceptInviation(otherAgent);
            StartAction(Disagreement);
        }
        else
        {
            // An agent is convinced to Communication based on its conscientousness trait.
            // Agents high on consciousness are more difficult to convince/distract
            double x = random.Next(100) / 100.0;
            LogDebug(String.Format("Agent proposal {0} >= {1} ...", x, personality.agreeableness * happiness));
            if (x >= (personality.agreeableness * happiness))
            {
                LogDebug(String.Format("Agent got convinced by {0} to start Disagreementing ...", otherAgent));
                Disagreement Disagreement = (Disagreement)behaviors["Disagreement"];
                Disagreement.acceptInviation(otherAgent);
                StartAction(Disagreement, false, false);
            }
            else
            {
                LogDebug(String.Format("Agent keeps to current action ({0} < {1})", x, personality.agreeableness * happiness));
            }
        }
    }

    public string GetScores()
    {
        StringBuilder sb = new StringBuilder();
        for (int actionidx = 0; actionidx < behaviors.Count; actionidx++)
        {
            AgentBehavior behavior = behaviors.Values.ElementAt(actionidx);

            if(behavior.name!="Rest")
            sb.Append(String.Format($"{behavior.name}:{this.scores[actionidx]:N2}\t"));
        }
        return sb.ToString();
    }

    private void SelectAction()
    {
        AgentBehavior best_action = null;

        CalculateActionScores();
        best_action = selectAction(scores);

        if (best_action != null)
        {
            bool success = StartAction(best_action);
            if (success)
            {
                LogInfo(String.Format("Starting Action {0}.", best_action));

                CalculateActionCount(best_action);
            }
            else
            {
                Debug.Log("Starting Action failed:" + best_action);

                LogInfo(String.Format("Starting Action {0} failed!", best_action));
                //throw new Exception("This must not happen!");
            }
        }
    }


    // Main Logic
    private void CalculateActionScores()
    {
        double rating = 0;
        AgentBehavior behavior = null;

        // Agents high on consciousness will stick longer to chosen actions
        // Look at:
        // https://www.wolframalpha.com/input/?i=plot+3.0+*+e**(-(1.0-0.3)*x)+from+x%3D0+to+5
        double score_bias = 0;
        //score_bias = (int)(ACTION_SCORE_BIAS * Math.Exp(-(1.0 - personality.conscientousness) * (float)ticksOnThisTask));
        score_bias = (int)(SC.Agent["ACTION_SCORE_BIAS"] * Math.Exp(-(1.0 - personality.conscientousness) * SC.Agent["ACTION_SCORE_DECAY"] * (float)ticksOnThisTask));

        for (int actionidx = 0; actionidx < behaviors.Count; actionidx++)
        {
            behavior = behaviors.Values.ElementAt(actionidx);
            rating = behavior.rate();

            // The current action gets a score boost that declines exponentially
            if (behavior == currentAction)
            {
                rating += score_bias;
            }

            if (behavior == previousAction)
            {
                rating -= score_bias;
            }
            scores[actionidx] = rating;
        }
        LogInfo("Scores: " + GetScores());

        // Calculate a weighted sum of individual and peer action score (weighted by conformity)
        scores = scores.Zip(classroom.peerActionScores, (x, y) => x * (1.0 - conformity) + y * conformity).ToArray();
    }

    private AgentBehavior selectAction(double[] scores)
    {
        // Chose action based on score
        int chosen_action = 0;
        int prob_action = ChooseActionByDistribution(scores);
        //int max_action = System.Array.IndexOf(scores, scores.Max());
        chosen_action = prob_action;

        return behaviors.Values.ElementAt(chosen_action);
    }

    // Return the index of an action in score, based on its probability/ratio of the score
    // This is implemented by generating an array filled with action indexes
    // The number of entries for an action is defined by the score
    // One element of that array is chosen randomly (uniform) so that the probability of select and action is equal to its score (normalized by the sum of all scores)
    private int ChooseActionByDistribution(double[] ratings)
    {
        double sum = 0;
        for (int action = 0; action < ratings.Length; action++) {
            if (ratings[action] > 0)
            {
                sum += Math.Pow(ratings[action], 3);
            }
        }
        int[] distribution = new int[100];

        int counter = 0;
        for (int action = 0; action < ratings.Length; action++)
        {
            if (ratings[action] > 0)
            {
                int normalized_rating = (int)(((Math.Pow(ratings[action], 3)) / sum) * 100.0);
                for (int i = 0; i < normalized_rating; i++)
                {
                    distribution[counter + i] = action;
                }
                counter += normalized_rating;
            }
        }

        // Chose a random element from the action distribution
        return distribution[random.Next(counter)];
    }

    public void CalculateActionCount(AgentBehavior behavior) {
        switch (behavior.action) {
            case AgentBehavior.Actions.SoloTime:
                actionCount.soloCount++;
                break;
            case AgentBehavior.Actions.InteractionTime:
                actionCount.interactionCount++;
                actionCount.totalCount++;
                break;
            case AgentBehavior.Actions.Rest:
                actionCount.restCount++;
                break;
            case AgentBehavior.Actions.Communication:
                actionCount.communicationCount++;
                actionCount.totalCount++;
                break;
            case AgentBehavior.Actions.Disagreement:
                actionCount.disagreementCount++;
                break;
        }
    }
    public void AddFriends(Agent otherAgent)
    {
        // No Friend :(


        if (friendLists.Count < 1)
        {
            Friend friend = new Friend(otherAgent, 1);
            if (!friendLists.Contains(friend))
            {
                friendLists.Add(friend);
            }
            Debug.Log(String.Format("****{0} GOT THE FIRST FRIEND :{1} ", personality.name, otherAgent.personality.name));
        }
        else
        {
            var result = friendLists.Where(t => t.agent && t.agent.Equals(otherAgent));

            Debug.Log("****Duplicated Friends****:" + result.Count());
            if (result.Count() == 0)
            {
                // No duplicate
                Friend friend = new Friend(otherAgent, 1);
                if (!friendLists.Contains(friend))
                {
                    friendLists.Add(friend);
                }
                Debug.Log(String.Format("****{0} GOT A NEW FRIEND :{1} ", personality.name, otherAgent.personality.name));
            }
            else {
                foreach (Friend f in result)
                {
                    Debug.Log(String.Format("****{0} GOT A Repeated FRIEND :{1} already In friendLists {2} ", personality.name, otherAgent.personality.name, f.agent.personality.name));
                    f.count++;
                }
            }

        }
    }
    public void SetGptName() {
        int randNum = UnityEngine.Random.Range(1, personality.additionalTags.Length);
        gptName = string.Format("GPT:{0}", personality.additionalTags[randNum]);
    }
    public void ResetActionCount() {
        actionCount = new ActionCount();
    }
}
