import hashlib
import os

def getHashMd5(filename):
    m = hashlib.md5()
    with open(filename, 'rb') as f:
        while True:
            data = f.read(8192)
            if not data:
                break
            m.update(data)
        return m.hexdigest()

def getFileList(dirName):
    fileList = []
    for root, _, files in os.walk(os.getcwd() + dirName):
        for file in files:
            fileList.append(root + '/' + file)
    return fileList

def genList():
    dirName = '/dir'
    try:
        fileList = getFileList(dirName)
        hashList = ''
        for file in fileList:
            hashList += file[len(os.getcwd() + dirName) + 1::] + '|' + getHashMd5(file) + '|' + str(os.path.getsize(file)) + '\n'
        f = open('list.txt','w+')
        print(hashList)
        f.write(hashList)
        f.close()
        return True
    except:
        return False

if __name__ == "__main__":
    genList()
    