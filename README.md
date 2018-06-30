# DymeRules

##What is it: 
A collection of tools for inference and rule building.

##Where or when should I use it? 
Validate configuration files. Find hidden relationships between data.

##How do I use it? 
Create some rules about Json objects, and then use the rules to validate a set of Json objects.

##Using library "JsonEasyRules"
	
Example rules:
	Rule: 		"if (sky) is (blue) then (planet) is (Earth)"

Example world (Json objects):
	Config1:	{ "planet": "Earth", "sky": "blue" }
	Config2:	{ "planet": "Mars",  "sky": "blue" }

World1(Earth) will return true, and World2(Mars) will return false.
This is because according to our rule, the planet must be Earth if the sky is blue,
therefore World2 fails because the sky is blue, but the planet is Mars.
If these were two actual configs, then there would be something wrong with config 2.
The knowledge that Mars's sky is not blue, is existential knowledge that your system doesn't know about.
Having a rule, is a way of encoding that knowledge, and by running the rule engine over your configs when you do a deployment,
you can automatically ensure that those types of defects don't make their way into your live environment.

The Easy-Rules syntax is meant to be as close to natural language as possible,
making rules are easy to read and write.