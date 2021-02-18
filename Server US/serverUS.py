import socket
from threading import Thread

sock = socket.socket()
sock.bind(('', 9090))
sock.listen(256)

def AcceptClients(conn, addr):
	try:
		print('connected:', addr)
		while True:
			data = conn.recv(2)
			if not data:
				break
			a = int(data[0])
			b = int(data[1])
			print(str(a) + ' + ' + str(b) + ' = ' + str(a + b))
			conn.send(bytes([a + b]))
	except:
		print('disconnected:', addr)
		conn.close()


if __name__ == '__main__':
	while True:
		th = Thread(target=AcceptClients, args=sock.accept())
		th.run()