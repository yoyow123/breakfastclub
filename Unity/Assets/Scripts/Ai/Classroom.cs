using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

using System.IO;
using System.Text;

[Serializable]
public class sampleConfig
{
    public string name;
    public int seed;
    public int ticks;
    public double timescale;
    public PersonalityType[] agent_types;
    public int[] nAgents;
}


// Helper struct that is used to generate SimulationConfig object from json file
[Serializable]
public struct SerializableSimulationConfig
{
    public string name;
    public NamedConfigValue[] Classroom;
    public NamedConfigValue[] Agent;
    public NamedConfigValue[] AgentBehavior;

    public NamedConfigValue[] Communication;
    public NamedConfigValue[] Rest;
    public NamedConfigValue[] Disagreement;
    public NamedConfigValue[] InteractionTime;
    public NamedConfigValue[] SoloTime;
}

[Serializable]
public struct NamedConfigValue
{
    public string field;
    public double value;
    public NamedConfigValue(string field, double value)
    {
        this.field = field;
        this.value = value;
    }
}

public class SimulationConfig
{
    public string name;
    public Dictionary<string, double> Classroom;
    public Dictionary<string, double> Agent;
    public Dictionary<string, double> AgentBehavior;
    public Dictionary<string, double> Communication;
    public Dictionary<string, double> Rest;
    public Dictionary<string, double> Disagreement;
    public Dictionary<string, double> InteractionTime;
    public Dictionary<string, double> SoloTime;

    public SimulationConfig(SerializableSimulationConfig sSC)
    {
        this.name = sSC.name;
        this.Classroom = List2Dict(sSC.Classroom);
        this.Agent = List2Dict(sSC.Agent);
        this.AgentBehavior = List2Dict(sSC.AgentBehavior);
        this.Communication = List2Dict(sSC.Communication);
        this.Rest= List2Dict(sSC.Rest);
        this.Disagreement = List2Dict(sSC.Disagreement); 
        this.InteractionTime = List2Dict(sSC.InteractionTime); 
        this.SoloTime = List2Dict(sSC.SoloTime);

    }

    private Dictionary<string, double>List2Dict(NamedConfigValue[] list)
    {
        Dictionary<string, double> dict = new Dictionary<string, double>();
        list.ToList().ForEach(x => dict.Add(x.field, x.value));
        return dict;
    }
}

public class Classroom : MonoBehaviour
{
    public double noise { get; protected set; }

    //public string configfile = "ConfigFile.json";
    public string simulationConfigFile = "SimulationConfigFile.json";
    public string sampleConfigFile = "sample_personas_config.json";
    public TMPro.TextMeshProUGUI tickCounterText;
    public TMPro.TextMeshProUGUI onScreenLogText;

    [NonSerialized] public Table[] groupTables;
    [NonSerialized] public Table[] individualTables;
    [NonSerialized] public AgentSpawner[] AgentSpawners;
    public AgentSpawner asp;

    public Agent[] agents;
    [NonSerialized] public bool gamePaused = false;
    public Transform groundfloorTransform;

    private GlobalRefs GR;
    private CSVLogger Logger;
    [HideInInspector] public double turnCnt = 0;
    private int commandline_seed = 0;

    //private GameConfig gameConfig = new GameConfig();
    public SimulationConfig simulationConfig;
    public SampleConfig sampleConfig = new SampleConfig();

    public double[] peerActionScores { get; private set; }

    private double motivation_mean;
    private double motivation_std;
    private double happiness_mean;
    private double happiness_std;
    private double attention_mean;
    private double attention_std;

    System.Random random;

    // Start is called before the first frame update
    void Start()
    {
        noise = 0.0;
        //UnityEngine.Random.InitState(42);

        GetReferences();

        try { 
            ParseCommandLine(); 
        } catch { Debug.Log("Parsing command line failed!"); };


        LoadSimulationConfig(simulationConfigFile);
        LoadSampleConfig(sampleConfigFile);

        SpawnAgents();

        // Find all Agents
        agents = FindObjectsOfType<Agent>();

       

        peerActionScores = new double[agents[0].scores.Length];

        //onScreenLogText.text = $"Seed {sampleConfig.seed}\nConfig file {configfile}";
        Debug.Log($"Seed: {sampleConfig.seed}\nConfig file: {sampleConfigFile}\nAgents:{sampleConfig.nAgents.Length}");
    }

    void SetScreenMessage(string message)
    {
        onScreenLogText.text = message;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        turnCnt++;

        Debug.Log($"Turn: {turnCnt}");

        // Update this list every turn (agents might leave or enter the classroom)
        agents = FindObjectsOfType<Agent>();

        UpdateStats();

        peerActionScores = GetPeerActionScore(agents);

        LogStats();

        if ((sampleConfig.ticks > 0) && (turnCnt >= sampleConfig.ticks))
        {
            EndSimulation();
        }
    }

    private void GetReferences()
    {
        GR = GlobalRefs.Instance;
        Logger = GR.logger;
       // groundfloorTransform = transform.Find("Groundfloor").GetComponent<Transform>();

        // Find all Tables
        groupTables = Array.ConvertAll(GameObject.FindGameObjectsWithTag("GTable"), item => item.GetComponent<Table>());
        individualTables = Array.ConvertAll(GameObject.FindGameObjectsWithTag("ITable"), item => item.GetComponent<Table>());

        // Find all Agent Spawners
        AgentSpawners = FindObjectsOfType<AgentSpawner>();
    }

    private void ParseCommandLine()
    {
        // Dont do any parsing if we are in editor mode
        if (Application.isEditor)
            return;

        string[] args = System.Environment.GetCommandLineArgs();
        // Filter all args that start with -
        List<string> filtered_args = new List<string>();
        foreach (string s in args)
        {
            //Debug.Log("******* :"+s);
            if (s[0] != '-')
            {
                filtered_args.Add(s);
            }
        }
        //onScreenLogText.text = string.Join(" ", filtered_args);

        try
        {
            simulationConfigFile = filtered_args[1];
            sampleConfigFile = filtered_args[2];
            commandline_seed = int.Parse(filtered_args[3]);
            string logfilepath = filtered_args[4];

            Logger.setLogfile(logfilepath);
        }
        catch {
            Debug.LogError("Parsing Command line arguments failed!");
            //Application.Quit();
        };
    }

    private void LoadSampleConfig(string configpath)
    {
        //createsampleConfig("NewsampleConfig.json");
        Debug.Log($"Reading classroom config from {configpath} ...");
        string config = System.IO.File.ReadAllText(@configpath);
        sampleConfig = JsonUtility.FromJson<SampleConfig>(config);

        // Command line seed overwrites config seed!
        if(commandline_seed != 0)
        { 
            sampleConfig.seed = commandline_seed;
        }
        random = new System.Random(sampleConfig.seed);
        Time.timeScale = (float)sampleConfig.timescale;
    }

    private void LoadSimulationConfig(string configpath)
    {
        try
        {
            Debug.Log($"Reading classroom config from {configpath} ...");
            string json = System.IO.File.ReadAllText(@configpath);
            simulationConfig = new SimulationConfig(JsonUtility.FromJson<SerializableSimulationConfig>(json));
        } catch {
            Debug.LogError("Loading Simulation Config failed!");
            //Application.Quit();
        };

    }

 /*   private void LoadTagsConfig(string configpath)
    {
        try
        {
            Debug.Log($"Reading classroom config from {configpath} ...");
*//*            if (File.Exists(configpath))
                Debug.Log("**Config file exists.");
            else
                Debug.Log("** No Config file is found.");*//*

            string json = System.IO.File.ReadAllText(@configpath);
            sampleConfig = JsonUtility.FromJson<SampleConfig>(json);
        }
        catch
        {
            Debug.LogError("Loading Tag Config failed!");
            //Application.Quit();
        };

    }*/

    private void SpawnAgents()
    {
        int nAgents = 0;

        for (int k = 0; k < sampleConfig.agent_types.Length; k++)
        {
            int newseed = random.Next();
            System.Random newRandom = new System.Random(newseed);
            //AgentSpawner asp = AgentSpawners[random.Next(AgentSpawners.Length)];
            Personality p = new Personality(sampleConfig.agent_types[k]);
            Debug.Log($"Spawning Agent {nAgents} with seed {newseed} ...");
            //  GameObject newAgent = asp.SpawnAgent(newRandom, p);
            GameObject n_agent = asp.SpawnAgent(sampleConfig.agent_types[k].name, newRandom, p);
            n_agent.name = $"Agent{nAgents:D2}";
            nAgents++;
        }

    }


    // Used to create a teamplate json config file that later can be edited by hand
    /*private void createsampleConfig(string filename)
    {
        sampleConfig gc = new sampleConfig();
        gc.name = "TestConfig";
        gc.seed = 42;
        gc.ticks = 100;
        gc.timescale = 100.0;
        gc.agent_types = new PersonalityType[2];
        gc.agent_types[0] = new PersonalityType("Type1", 0.8, 0.6, -1, -1, 0.6);
        gc.agent_types[1] = new PersonalityType("Type2", 0.6, 0.5, 0.8, 0.8, 0.2);
        gc.nAgents = new int[2];
        gc.nAgents[0] = 2;
        gc.nAgents[1] = 3;


        string json = JsonUtility.ToJson(gc);
        StreamWriter sw = new StreamWriter(filename);
        sw.Write(json);
        sw.Close();
    }
*/
    private void Update()
    {
        tickCounterText.text = $"Tick: {(int)turnCnt:D}\nNoise: {noise:F2}";
        //Debug.Log("Update time :" + Time.deltaTime);
        if (Input.GetKeyDown("space"))
        {
            if (gamePaused)
            {
                Debug.Log("Resume Game");
                Time.timeScale = (float)sampleConfig.timescale;
                SetScreenMessage("");
            }
            else
            {
                Debug.Log("Pause Game");
                Time.timeScale = 0.0f;
                SetScreenMessage("Simulation Paused");
            }
            Debug.Log("Tag Configs:" + sampleConfig.agent_types[0].name);
            Debug.Log("Tag Configs : " + sampleConfig.name);
            Debug.Log("Tag Configs : " + sampleConfig.seed);
            gamePaused = !gamePaused;
        }
        else if (Input.GetKeyDown("q"))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown("r")) {
            Debug.Log("Reload config");
            Reload();

        }
    }



    public void EndSimulation()
    {
        Debug.Log("Ending Simulation!");
        //Application.Quit();
    }

    public double Mean(List<double> values)
    {
        return values.Sum()/values.Count();
    }

    public double Variance(List<double> values, double meanvalue)
    {
        double variance = 0.0;
        foreach(double v in values){
            variance += (v-meanvalue)*(v-meanvalue);
        }
        variance = variance / values.Count();
        return variance;
    }

    public double Std(List<double> values)
    {
        double m = Mean(values);
        double v = Variance(values, m);
        return Math.Sqrt(v);
    }

    public void LogX(string message, string type)
    {
        string[] msg = { gameObject.name, turnCnt.ToString(), type, message };
        Logger.log(msg);
    }

    public void LogStats(bool include_info_log = true)
    {
        if (include_info_log)
        {
            LogX(String.Format($"#Agents {agents.Count()} | Noise {noise} | Energy_mean {motivation_mean} | Energy_std {motivation_std} | Happiness_mean {happiness_mean} | Happiness_std {happiness_std} | Attention_mean {attention_mean} | Attention_std {attention_std}"), "I");
        }
        LogX(String.Format($"{agents.Count()}|{noise}|{motivation_mean}|{motivation_std}|{happiness_mean}|{happiness_std}|{attention_mean}|{attention_std}"), "S");

    }

    public void UpdateStats()
    {
        (motivation_mean, motivation_std) = GetAccEnergy();
        (happiness_mean, happiness_std) = GetAccHappiness();
        (attention_mean, attention_std) = GetAccAttention();

        //noise = AgentBehavior.boundValue(0.0, GetAccNoise(), 1.0); 
        noise = GetAccNoise();
    }

    private double GetAccNoise()
    {
        // Reset noise

        double noise_inc = 0.0;
        foreach (var agent in agents)
        {
            noise_inc += agent.currentAction.noise_inc;
        }
        return noise_inc;
    }

    private (double, double) GetAccEnergy()
    {
        double mean = 0.0;
        double std = 0.0;

        List<double> values = new List<double>();
        foreach(Agent agent in agents)
        {
            values.Add(agent.motivation);
        }
        mean = Mean(values);
        std = Std(values);

        return (mean, std);
    }

    private (double, double) GetAccHappiness()
    {
        double mean = 0.0;
        double std = 0.0;

        List<double> values = new List<double>();
        foreach (Agent agent in agents)
        {
            values.Add(agent.happiness);
        }
        mean = Mean(values);
        std = Std(values);

        return (mean, std);
    }

    private (double, double) GetAccAttention()
    {
        double mean = 0.0;
        double std = 0.0;

        List<double> values = new List<double>();
        foreach (Agent agent in agents)
        {
            if((agent.currentAction is SoloTime) || (agent.currentAction is InteractionTime))
            {
                values.Add(agent.attention);
            }
        }

        //if (values.Count() > 0)
        if (values.Any())
        {
            mean = Mean(values);
            std = Std(values);
        }
        else
        {
            mean = 0.0;
            std = 0.0;
        }

        return (mean, std);
    }


    // Calculate a actino score vectoring representing the classroom interest
    private double[] GetPeerActionScore(Agent[] agents)
    {
        double[] scores = new double[agents[0].scores.Length];

        // Add scores
        foreach (var agent in agents)
        {
            scores = scores.Zip(agent.scores, (x, y) => x + y).ToArray();
        }

        // Normalize them, giving equal wheight to each agent (-> Flath dominace hierarchy)
        scores = scores.Select(x => x / agents.Length).ToArray();

        return scores;
    }

    public List<Agent> getAgentsByDesire(AgentBehavior filter)
    {
        List<Agent> available = new List<Agent>();
        foreach (var agent in agents)
        {
            if(agent.Desire.Equals(filter))
            {
                available.Add(agent);
            }
        }
        return available;
    }


    public List<Agent> getAgentsByAction(AgentBehavior filter)
    {
        List<Agent> available = new List<Agent>();
        foreach (var agent in agents)
        {
            if (agent.currentAction.Equals(filter))
            {
                available.Add(agent);
            }
        }
        return available;
    }

    public void Reload() {

        LoadSampleConfig(sampleConfigFile);
        foreach (Agent a in agents) {
            //reset action count as zero
            a.ResetActionCount();
            Destroy(a.gameObject);
        }
        SpawnAgents();
        agents = FindObjectsOfType<Agent>();

    }
    
      
}
