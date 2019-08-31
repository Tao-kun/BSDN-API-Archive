import requests
import hashlib

filename=r'C:\Users\zhuo\Desktop\media\1f9bada3.jpg'
with open(filename,"rb") as f:
	file_content=f.read()

md5=hashlib.md5(file_content).hexdigest()
print(md5)

files={'file':(filename,file_content)}

r=requests.post("http://127.0.0.1:5000/api/file?token=83f6df4726f552a67605404239f9f1cd",files=files)
print(r.ok)
print(r.text)