class serial_left:
	def write(self,string):
		send_left(string)

class serial_right:
	def write(self,string):
		send_right(string)

sl = serial_left()
sr = serial_right()

locals={
	"FREEMV_SERIAL": sl
}

execfile(File_Path, locals)

