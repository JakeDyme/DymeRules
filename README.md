# DymeRules

#### What is it: 
A collection of tools for inference and rule building.

#### Where or when should I use it? 
Validate configuration files. Find hidden relationships between data.

#### How do I use it? 
Create some rules about Json objects, and then use the rules to validate a set of Json objects.

## Using library "JsonEasyRules"
	
#### Example rules:
---
	Rule: 		"if (sky) is (blue) then (planet) is (Earth)"

#### Example world (Json objects):
---
	Config1:	{ "planet": "Earth", "sky": "blue" }
	Config2:	{ "planet": "Mars",  "sky": "blue" }

World1(Earth) will return true, and World2(Mars) will return false.
This is because according to our rule, the planet must be Earth if the sky is blue,
therefore World2 fails because the sky is blue, but the planet is Mars.
If these were two actual configs, then there would be something wrong with config 2.

The knowledge that Mars's sky is not blue, is existential knowledge that your system doesn't know about.
Having a rule is a way of encoding that knowledge. 
Running the rule engine over your configs when you do a deployment allows you to automatically ensure that those types of defects don't make their way into your live environment.

## Easy-Rule syntax
The Easy-Rules syntax is meant to be as close to natural language as possible, making rules easy to read and write.

Constructs
-----------
- Implication: if ... then ...
- Conjunction: ... and ...
- Disjunction: ... or ...
- Proposition: (...) is|not|greater than|less than|contains|in (...)

### Proposition
---------------
FORMATS:
- unary comparison:  ([key]) operator ([value])
- binary comparison: ([key1]) operator (setting)([key2])

COMPARERS:
- equality: is | must be
- negation: not | is not
- inequality: greater than | is greater than | less than | is less than
- superset: contains
- subset: in

EXAMPLES:
- ("SKY_COLOR") is ("blue")
- ("SKY_COLOR") is (setting)("SEA_COLOR")
- ("SKY_COLOR") is not ("red")
- ("SKY_COLOR") is not (setting)("GROUND_COLOR")
- ("SKY_COLOR") in ("blueish")
- ("SKY_COLOR") contains ("blu")
- ("PLANET_AGE") greater than ("2000000")
- ("PLANET_AGE") less than (setting)("SUN_AGE")
