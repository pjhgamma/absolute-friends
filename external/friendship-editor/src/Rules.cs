using System.Globalization;
using System.Text;

namespace AbsoluteFriends.FriendshipEditor;

internal enum Stat
{
    Reputation,
    Like,
    TempLike,
    Know,
    Relationship,
}

internal enum RuleMode
{
    Vanilla,
    Minimum,
    Maximum,
    Fixed,
}

internal readonly struct RuleKey(string subject, string region, string target, Stat stat) : IEquatable<RuleKey>
{
    internal string Subject { get; } = subject;

    internal string Region { get; } = region;

    internal string Target { get; } = target;

    internal Stat Stat { get; } = stat;

    public bool Equals(RuleKey other) => Subject == other.Subject && Region == other.Region && Target == other.Target && Stat == other.Stat;

    public override bool Equals(object? obj) => obj is RuleKey other && Equals(other);

    public override int GetHashCode() => (Subject, Region, Target, Stat).GetHashCode();
}

internal readonly struct Rule(RuleMode mode, float value, string? relation = null)
{
    internal RuleMode Mode { get; } = mode;

    internal float Value { get; } = value;

    internal string? Relation { get; } = relation;

    internal bool Enforces => Mode != RuleMode.Vanilla;

    internal float Apply(float value) => Mode switch
    {
        RuleMode.Minimum => Clamp(Math.Max(value, Value)),
        RuleMode.Maximum => Clamp(Math.Min(value, Value)),
        RuleMode.Fixed => Clamp(Value),
        _ => value,
    };

    internal bool IsValid(Stat stat) => stat == Stat.Relationship
        ? Mode == RuleMode.Vanilla ? Relation == null : Mode == RuleMode.Fixed && RelationshipTypes.Find(Relation) != null && Value >= 0f && Value <= 1f
        : Relation == null && (!Enforces || Value >= (stat == Stat.Know ? 0f : -1f) && Value <= 1f);

    private static float Clamp(float value) => Math.Max(-1f, Math.Min(1f, value));
}

internal sealed class Rules
{
    internal const string All = "*";

    private readonly Dictionary<RuleKey, Rule> _rules = [];

    private readonly HashSet<string> _targets = [];

    internal bool IsEmpty => _rules.Count == 0;

    internal bool HasLikeRules { get; private set; }

    internal bool HasReputationRules { get; private set; }

    internal bool HasRelationshipRules { get; private set; }

    internal bool HasTarget(string target) => _targets.Contains(target);

    internal bool Any(Func<RuleKey, bool> match) => _rules.Keys.Any(match);

    internal Rule? Exact(RuleKey key) => _rules.TryGetValue(key, out Rule rule) ? rule : null;

    internal Rule? Resolve(RuleKey key)
    {
        if (_rules.Count == 0)
        {
            return null;
        }

        foreach (string candidateSubject in Candidates(key.Subject))
        {
            foreach (string candidateRegion in Candidates(key.Region))
            {
                foreach (string candidateTarget in Candidates(key.Target))
                {
                    if (Exact(new(candidateSubject, candidateRegion, candidateTarget, key.Stat)) is { } rule)
                    {
                        return rule;
                    }
                }
            }
        }

        return null;
    }

    internal void Set(RuleKey key, Rule? rule)
    {
        if (rule is not { } configured)
        {
            _rules.Remove(key);
        }
        else if (Enum.IsDefined(typeof(Stat), key.Stat) && Enum.IsDefined(typeof(RuleMode), configured.Mode) && configured.IsValid(key.Stat))
        {
            _rules[key] = configured;
        }
        else
        {
            throw new ArgumentException("Invalid friendship rule");
        }

        Index();
    }

    internal IEnumerable<(RuleKey Key, Rule Rule)> Matching(Func<RuleKey, bool> match) => _rules.Where(pair => match(pair.Key)).Select(pair => (pair.Key, pair.Value));

    private void Index()
    {
        HasLikeRules = _rules.Keys.Any(entry => entry.Stat is Stat.Like or Stat.TempLike or Stat.Know);
        HasReputationRules = _rules.Keys.Any(entry => entry.Stat == Stat.Reputation);
        HasRelationshipRules = _rules.Keys.Any(entry => entry.Stat == Stat.Relationship);
        _targets.Clear();
        _targets.UnionWith(_rules.Keys.Select(entry => entry.Target));
    }

    internal string Serialize() => Encode(string.Join("\n", _rules
        .OrderBy(pair => pair.Key.Stat)
        .ThenBy(pair => pair.Key.Subject, StringComparer.Ordinal)
        .ThenBy(pair => pair.Key.Region, StringComparer.Ordinal)
        .ThenBy(pair => pair.Key.Target, StringComparer.Ordinal)
        .Select(pair => string.Join("\t", Fields(pair.Key, pair.Value)))));

    internal static Rules Deserialize(string content)
    {
        Rules rules = new();

        try
        {
            foreach (string line in Decode(content).Split('\n'))
            {
                string[] fields = line.Split('\t');

                if (fields.Length < 6
                    || !Enum.TryParse(fields[3], out Stat stat)
                    || !Enum.TryParse(fields[4], out RuleMode mode)
                    || !float.TryParse(fields[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                {
                    continue;
                }

                try
                {
                    rules.Set(new(Decode(fields[0]), Decode(fields[1]), Decode(fields[2]), stat), new(mode, value, fields.Length > 6 ? Decode(fields[6]) : null));
                }
                catch (ArgumentException)
                {
                }
                catch (FormatException)
                {
                }
            }
        }
        catch (FormatException)
        {
        }

        return rules;
    }

    private static string[] Fields(RuleKey key, Rule rule) =>
    [
        Encode(key.Subject),
        Encode(key.Region),
        Encode(key.Target),
        key.Stat.ToString(),
        rule.Mode.ToString(),
        rule.Value.ToString("R", CultureInfo.InvariantCulture),
        .. rule.Relation is { } relation ? [Encode(relation)] : Array.Empty<string>(),
    ];

    private static IEnumerable<string> Candidates(string value) => value == All ? [All] : [value, All];

    private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    private static string Decode(string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));
}
