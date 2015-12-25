#Descriptions:
# Interface Definiton
# locals={
#	"Custom1": s1
#	"Custom2": s2
#}
#

class Serial1:
	def write(self,string):
		SEND1(string)

class Serial2:
	def write(self,string):
		SEND2(string)

s1 = Serial1()
s2 = Serial2()


#---------------Custom Section----------------
locals={
	"FREEMV_SERIAL": s1,
	"FREEMV_ASSISTANT_SERIAL": s2
}
#---------------------------------------------


execfile(ScriptPath, locals)

