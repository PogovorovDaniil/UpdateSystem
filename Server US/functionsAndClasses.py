import hashlib

def get_hash_md5(filename):
    with open(filename, 'rb') as f:
        m = hashlib.md5()
        while True:
            data = f.read(8192)
            if not data:
                break
            m.update(data)
        return m.hexdigest()

class testClass(object):
    def __init__(self):
        self.a = 5
    def aplusplus(self):
        self.a += 1