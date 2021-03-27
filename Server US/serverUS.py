# -- coding: cp1251 --

import socket
import os
from threading import Thread
import HashListMD5

sock = socket.socket()
sock.bind(('', 9090))
sock.listen(256)

HashListMD5.genList()
print('Hash generated')
flist = open('list.txt','rb')
fignore = open('ignore.txt','rb')
baseDir = os.getcwd() + '/dir/'

def SendFile(conn):
	filelen = conn.recv(1)
	pathbytes = conn.recv(int(filelen[0]))
	path = pathbytes.decode('utf-8', 'replace')

	if os.path.abspath(baseDir + path).find(baseDir) == -1:
		return 1
	if not os.path.isfile(baseDir + path):
		return 1

	sendF = open(baseDir + path, 'rb')
	sendF.seek(0,2)
	size = sendF.tell()
	conn.send(bytes([ int(size / 0x1000000) % 0x100, int(size / 0x10000) % 0x100, int(size / 0x100) % 0x100, int(size) % 0x100]))

	sendF.seek(0)
	fileBytes = sendF.read(4096)
	while fileBytes:
		conn.send(fileBytes)
		fileBytes = sendF.read(4096)
	sendF.close()
	return 0

def SendFileList(conn):
	flist.seek(0,2)
	size = flist.tell()
	print('Send ', size, ' bytes')
	conn.send(bytes([ int(size / 0x1000000) % 0x100, int(size / 0x10000) % 0x100, int(size / 0x100) % 0x100, int(size) % 0x100]))

	flist.seek(0)
	fileBytes = flist.read(4096)
	while fileBytes:
		conn.send(fileBytes)
		fileBytes = flist.read(4096)
	return 0

def SendIgnoreList(conn):
	fignore.seek(0,2)
	size = fignore.tell()
	print('Send ', size, ' bytes')
	conn.send(bytes([ int(size / 0x1000000) % 0x100, int(size / 0x10000) % 0x100, int(size / 0x100) % 0x100, int(size) % 0x100]))

	fignore.seek(0)
	fileBytes = fignore.read(4096)
	while fileBytes:
		conn.send(fileBytes)
		fileBytes = fignore.read(4096)
	return 0

serverActions = [SendFileList, SendFile, SendIgnoreList]

def AcceptClients(conn, addr):
	try:
		print('connected:', addr)
		while True:
			data = conn.recv(1)
			if not data:
				break
			actId = int(data[0])
			
			if actId >= len(serverActions) or actId < 0:
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