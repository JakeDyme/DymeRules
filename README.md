# DymeRules

#### What is it: 
A collection of tools for inference and rule building.

#### Where or when should I use it? 
Validate configuration files. Find hidden relationships between data.

#### How do I use it? 
Create some rules about Json objects, and then use the rules to validate a set of Json objects.

## Using library "JsonEasyRules"
	
#### Example rules: (in Easy-Rule syntax)
---
	var rule =	"if (sky) is (blue) then (planet) is (Earth)";

#### Example worlds (Json objects):
---
	var earthConfig = "{ 'planet': 'Earth', 'sky': 'blue' }";
	var marsConfig  = "{ 'planet': 'Mars',  'sky': 'blue' }";
	var venusConfig = "{ 'planet': 'Venus', 'sky': 'orange' }";

#### Example usage:
---
	var evaluator = JsonEasyRuleEvaluator.CreateEvaluator();
	var validEarth = evaluator.IsTrueIn(rule, earthConfig);
	var validMars  = evaluator.IsTrueIn(rule, marsConfig);
	var validVenus = evaluator.IsTrueIn(rule, marsConfig);
	
#### Returns:
--------------
	validEarth = true;  *(because the sky is blue in this world and the planet's name is Earth)*
	validMars  = false; *(because the sky is blue in this world but the planets name is Mars)*
	validVenus = true;  *(because the sky is not blue)*

#### Explanation
----------------
World1(Earth) will return true, and World2(Mars) will return false.
This is because according to our rule, the planet must be Earth if the sky is blue,
therefore World2 fails because the sky is blue, but the planet is Mars.
If these were two actual configs, then there would be something wrong with config 2.
Venus evaluates to true because the sky is orange, so immediately the rule says that this config is fine because the rule simply doesn't apply to this world. 

#### Conclusion
---------------
The knowledge that Mars's sky is not blue, is knowledge that your system doesn't know about, but you know about it.
Having a rule is a way of encoding that knowledge. 
Running the rule engine over your configs when you do a deployment allows you to automatically ensure that those types of defects don't make their way into your live environment.

## Using JSON-Path.
---
#### Sample Json
<pre><code>
{
	'Stores': [
	'Lambton Quay',
	'Willis Street'
	],
	'Manufacturers': [
		{
		  'Name': 'Acme Co',
		  'Products': [
		    {
		      'Name': 'Anvil',
		      'Price': 50
		    }
		  ]
		},
		{
		  'Name': 'Contoso',
		  'Products': [
		    {
		      'Name': 'Elbow Grease',
		      'Price': 99.95
		    },
		    {
		      'Name': 'Headlight Fluid',
		      'Price': 4
		    }
		  ]
		}
	]
}
</code></pre>
#### Example Rules (using Json-Path as attribute names)

- `IF ($.Manufacturers[?(@.Name == 'Acme Co')].Products[0].Name) IS (Anvil)`
  <br />`THEN ($.Manufacturers[?(@.Name == 'Acme Co')].Products[0].Price) IS (50)")`
<br />**returns true**

- `if ($.Stores[0]) is (Lambton Quay)`
  <br />`then ($.Manufacturers[0].Products[0].Price) is greater than (49.36)` 
  <br />`AND ($.Manufacturers[0].Products[0].Price) is less than (51)")`
<br />**returns true**

## Easy-Rule syntax
---
The Easy-Rules syntax is meant to be as close to natural language as possible, making rules easy to read and write.

###Constructs
-----------
- Implication: `if ... then ...`
- Conjunction: `... and ...`
- Disjunction: `... or ...`
- Proposition: `(...) is|not|greater than|less than|contains|in (...)`

#### Examples:
- proposition: `(cat) is (grumpy)`
- implication: `if (bowl) is (empty) then (cat) is (grumpy)`
- conjunction: `(bowl) is (empty) and (cat) is (grumpy)`
- disjunction: `(bowl) is (full) or (cat) is (grumpy)`
##### Nested constructs
- `if (bowl) is (empty) and (cat) is (grumpy) then (desk) is (toilet)`



### Proposition
---------------

EXAMPLES OF EASY RULES:
- `(SKY_COLOR) is (blue)`
- `(SKY_COLOR) is (setting)(SEA_COLOR)`
- `(SKY_COLOR) is not (red)`
- `(SKY_COLOR) is not (setting)(GROUND_COLOR)`
- `(SKY_COLOR) in (blueish)`
- `(SKY_COLOR) contains (blu)`
- `(PLANET_AGE) greater than (2000000)`
- `(PLANET_AGE) less than (setting)(SUN_AGE)`
#### Formats (comparing settings to values, or settings to other settings) :
- unary comparison:  `([key])` `operator` `([value])`
- binary comparison: ([key1]) operator `(setting)`([key2])
#### Comparers:
- equality: `is` | `must be`
- negation: `not` | `is not`
- inequality: `greater than` | `is greater than` | `less than` | `is less than`
- superset: `contains`
- subset: `in`

### Implication
----------------
- `if (PLANET) is (Earth) then (sky) is (blue)`

### Conjunction
----------------
- `(PLANET) is (Earth) and (sky) is (blue)`

### Disjunction
----------------
- `(PLANET) is (Earth) or (sky) is (yellow)`

## Inference:
The "JsonEasyRules" library has some nice inference methods:

- `InferEasyRules` pass in a set of worlds, and get back a set of rules.
- `GetFailingRules` pass in a set of rules, and 1 world, and get back the rules that are failing in that world.
- `GetFailingWorlds` pass in a set of worlds, and 1 rule, and get back the worlds that are failing for that rule.

### How does the inference work?
There are many methods to infer rules from worlds. 
This one is fairly pessimistic. The following steps are used to infer the rules for this inference type.

1. Get the distinct list of facts from all worlds (a "fact" is a combination of an attribute and its value)
2. Exclude facts that are the same in every world.
3. Get all facts that repeat in more than one world.
4. Get all facts that repeat in the same worlds.
5. Convert into simple implications.

---
#### An example of how this works:
----------------------------------
We have the following worlds:
- world-1 {LOCATION:Home,  SHIRT:t-shirt, SHOES:open,   HAT: none }
- world-2 {LOCATION:Work,  SHIRT:button,  SHOES:closed, HAT: none }
- world-3 {LOCATION:Mall,  SHIRT:button,  SHOES:closed, HAT: none }
- world-4 {LOCATION:Shop,  SHIRT:t-shirt, SHOES:open,   HAT: none }
- world-5 {LOCATION:Home,  SHIRT:button,  SHOES:closed, HAT: none }
- world-6 {LOCATION:Shop, SHIRT:t-shirt, SHOES:open,   HAT: none }

####1. Get the distinct list of facts from all worlds 
(a "fact" is a combination of an attribute and its value)
- LOCATION:Home
- LOCATION:Work
- LOCATION:Ball
- LOCATION:Shop
- LOCATION:Beach
- SHIRT:t-shirt
- SHIRT:button
- SHOES:open
- SHOES:closed
- HAT:none

####2. Exclude facts that are the same in every world. 
We remove constants. They don't give us any clues about the state of the world since they are always the same. In this list "HAT:none" was a constant fact.
- LOCATION:Home
- LOCATION:Work
- LOCATION:Ball
- LOCATION:Shop
- LOCATION:Beach
- SHIRT:t-shirt
- SHIRT:button
- SHOES:open
- SHOES:closed

####3. Get all facts that repeat in more than one world.
- LOCATION:Home 	{World:1, World:5}
- LOCATION:Shop 	{World:4, World:6}
- SHIRT:t-shirt 	{World:1, World:4, World:6}
- SHIRT:button		{World:2, World:3, World:5}
- SHOES:open		{World:1, World:4, World:6}
- SHOES:closed		{World:2, World:3, World:5}

####4. Get all facts that repeat in the same worlds.
- (LOCATION:Home) repeats in {World:1, World:5}
- (SHOES:open)    repeats when (LOCATION:Shop) repeats in  {World:4, World:6}
- (SHIRT:t-shirt) repeats when (SHOES:open)    repeats in  {World:1, World:4, World:6}
- (SHOES:open)    repeats when (SHIRT:t-shirt) repeats in  {World:1, World:4, World:6}
- (SHIRT:button)  repeats when (SHOES:closed)  repeats in {World:2, World:3, World:5}
- (SHOES:closed)  repeats when (SHIRT:button)  repeats in {World:2, World:3, World:5}

####5. Convert into simple implications.
Since there must be at least two facts to build an implication, (LOCATION:Home) falls away.
It can be said that when (LOCATION) is (Shop) then (SHOES) are (open) since every time we're at the shop, our shoes are open
this implies that our shoes are open BECAUSE we're at the shop. We can therefore build an implication from this.
We do the same for all other repetitions.
The final rule-set emerges as:
- if (LOCATION:Shop) then (SHOES:open)
- if (SHIRT:t-shirt) then (SHOES:open)
- if (SHOES:open) then (SHIRT:t-shirt)
- if (SHIRT:button) then (SHOES:closed)
- if (SHOES:closed) then (SHIRT:button)
