import socket
from threading import Thread

sock = socket.socket()
sock.bind(('', 9090))
sock.listen(256)


def SendFileList(conn):
	print('SendFileList run')
	conn.send(bytes([1]))
	return 0

def SendVersion(conn):
	conn.send(bytes([2]))
	return 0

serverActions = [SendFileList, SendVersion]

def AcceptClients(conn, addr):
	try:
		print('connected:', addr)
		while True:
			data = conn.recv(1)
			if not data:
				break
			actId = int(data[0])
			
			if actId >= len(serverActions):
				raise ValueError('Bad actId')
			if serverActions[actId](conn):
				raise ValueError('Action error')

	except Exception as ex:
		print('disconnected:', addr, ' By reason: ', ex)
		conn.close()

def AcceptListener():
	while True:
		Thread(target=AcceptClients, args=sock.accept()).start()

if __name__ == '__main__':
	Thread(target=AcceptListener).start()