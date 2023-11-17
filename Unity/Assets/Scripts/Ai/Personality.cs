using System;

[Serializable]
public struct PersonalityType
{
    public string name;
    // Each Trial can have values between [0, 1]
    // A value of -1 will cause a random number to be used from a uniform distribution between [0, 1]
    public double openess;
    public double conscientousness;
    public double extraversion;
    public double agreeableness;
    public double neuroticism;
    public string color;
    public string typeName1;
    public string typeName2;
    public string roleName;
    public string roleDescription;
    public double incentives;
    public double engagement;
    public double focus;
    public string[] tags;
    public string[] additionalTags;

    public PersonalityType(string name, double o, double c, double e, double a, double n, 
                                         string color,string type1, string type2, string roleName, string roleDes,
                                        double incentives, double engage, double focus, string[] tags, string[] additionalTags)
    {
        this.name = name;
        openess = o;
        conscientousness = c;
        extraversion = e;
        agreeableness = a;
        neuroticism = n;
        this.color = color;
        typeName1 = type1;
        typeName2 = type2;
        this.roleName = roleName;
        this.roleDescription = roleDes;
        this.incentives = incentives;
        this.engagement = engage;
        this.focus = focus;
        this.tags = tags;
        this.additionalTags = additionalTags;
    }
}

[Serializable]
public class Personality
{
    public string name { get; protected set; }
    public double openess { get; protected set; }
    public double conscientousness { get; protected set; }
    public double extraversion { get; set; }
    public double agreeableness { get; protected set; }
    public double neuroticism { get; protected set; }
    public string color { get; protected set; }
    public string typeName1{ get; protected set; }

    public string typeName2 { get; protected set; }

    public string roleName { get; protected set; }

    public string roleDescription { get; protected set; }

    public double incentives { get; protected set; }

    public double engagement { get; protected set; }

    public double focus { get; protected set; }

    public string[] tags { get; protected set; }

    public string[] additionalTags { get; protected set; }


    public Personality(string name, double o, double c, double e, double a, double n)
    {
        this.name = name;
        openess = o;
        conscientousness = c;
        extraversion = e;
        agreeableness = a;
        neuroticism = n;
    }

    public Personality(PersonalityType pt) {
        name = pt.name;
        openess = pt.openess;
        conscientousness = pt.conscientousness;
        extraversion = pt.extraversion;
        agreeableness = pt.agreeableness;
        neuroticism = pt.neuroticism;
        color = pt.color;
        typeName1 = pt.typeName1;
        typeName2 = pt.typeName2;
        roleName = pt.roleName;
        roleDescription = pt.roleDescription;
        incentives = pt.incentives;
        engagement = pt.engagement;
        focus = pt.focus;
        tags = pt.tags;
        additionalTags = pt.additionalTags;
    }

    public Personality(Random random, PersonalityType pt)
    {
        name = pt.name;
        if (pt.openess < 0)
            openess = random.Next(100) / 100.0;
        else
            openess = pt.openess;

        if (pt.conscientousness < 0)
            conscientousness = random.Next(100) / 100.0;
        else
            conscientousness = pt.conscientousness;

        if (pt.extraversion < 0)
            extraversion = random.Next(100) / 100.0;
        else
            extraversion = pt.extraversion;

        if (pt.agreeableness < 0)
            agreeableness = random.Next(100) / 100.0;
        else
            agreeableness = pt.agreeableness;

        if (pt.neuroticism < 0)
            neuroticism = random.Next(100) / 100.0;
        else
            neuroticism = pt.neuroticism;
    }

    public Personality(Random random=null)
    {
        if (random == null){
            random = new Random();
        }

        name = "Random";
        neuroticism = random.Next(100) / 100.0;
        extraversion = random.Next(100) / 100.0;
        openess = random.Next(100) / 100.0;
        agreeableness = random.Next(100) / 100.0;
        conscientousness = random.Next(100) / 100.0;
    }

    public override string ToString() { return "T:" + name + " O:" + openess + " C:" + conscientousness + " E:" + extraversion + " A:" + agreeableness + " N:" + neuroticism; }
}
