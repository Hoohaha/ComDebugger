try:
	import os, sys, yaml
	import ConfigParser
except ImportError:
	pass

if __name__ == '__main__':
	ymlfile = sys.argv[1]
	with open(ymlfile, "r") as f:
		content = yaml.load(f)

	cf = ConfigParser.ConfigParser()
	cf.add_section("pattern")
	cf.add_section("no_pattern")
	cf.add_section("others")

	for n in xrange(len(content["pattern"])):
		cf.set("pattern",str(n), content["pattern"][n])

	for n in xrange(len(content["no_pattern"])):
		cf.set("no_pattern",str(n), content["no_pattern"][n])


	for key in content:
		if key != "pattern" and key !="no_pattern":
			cf.set('others', key, content[key])

	f = open("abc.ini",'w')
	cf.write(f)
	f.close()