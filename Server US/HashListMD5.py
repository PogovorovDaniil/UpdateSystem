import hashlib
import os

def getHashMd5(filename):
    with open(filename, 'rb') as f:
        m = hashlib.md5()
        while True:
            data = f.read(8192)
            if not data:
                break
            m.update(data)
        return m.hexdigest()

def getFileList(dirName):
    fileList = []
    for root, _, files in os.walk(dirName):
        for file in files:
            fileList.append(root + '\\' + file)
    return fileList

def genList():
    dirName = 'dir'
    try:
        fileList = getFileList(dirName)
        hashList = ''
        for file in fileList:
            hashList += file[len(dirName) + 1::] + '|' + getHashMd5(file) + '\n'
        f = open('list.txt','w')
        f.write(hashList)
        f.close()
        return True
    except:
        return False

if __name__ == "__main__":
    print('Ohh, da vi hacker')
    