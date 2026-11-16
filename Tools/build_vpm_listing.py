"""Build a static VPM listing without changing published release archives."""
import hashlib, io, json, pathlib, urllib.request, zipfile

ROOT = pathlib.Path(__file__).resolve().parents[1]
ID = 'com.lumakroma.modular-depth-animation-sps'
VERSION = '0.1.0-beta.3.rc.1'
BASE = 'https://lumakroma.github.io/modular-depth-animation/'
URL = 'https://github.com/LumaKroma/modular-depth-animation/releases/download/v0.1.0-beta.3.rc.1/ModularDepthAnimationForSPS-0.1.0-beta.3.rc.1-vpm.zip'
SHA = '463a768daa455b1a867f3fdf0ae2ee162e1406c8156cc7a0e61ab95ffe36161b'
archive = urllib.request.urlopen(URL).read()
assert hashlib.sha256(archive).hexdigest() == SHA
with zipfile.ZipFile(io.BytesIO(archive)) as z:
    assert z.namelist().count('package.json') == 1
    assert all(not n.startswith(('/', '../')) and '..' not in n.split('/') for n in z.namelist())
    manifest = json.loads(z.read('package.json'))
assert manifest['name'] == ID and manifest['version'] == VERSION
assert manifest['vpmDependencies'] == {'com.vrcfury.vrcfury':'1.1470.0', 'com.vrchat.avatars':'3.10.6', 'com.vrchat.base':'3.10.6'}
assert not any(k in manifest for k in ('legacyFolders','legacyFiles','legacyPackages'))
manifest.update(url=URL, zipSHA256=SHA)
listing = {'name':'LumaKroma Modular Depth Animation', 'id':'com.lumakroma.mda-listing', 'url':BASE+'index.json', 'author':'LumaKroma', 'packages':{ID:{'versions':{VERSION:manifest}}}}
out = ROOT / 'Website'
out.mkdir(exist_ok=True)
(out/'index.json').write_text(json.dumps(listing, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps({'package':ID,'version':VERSION,'sha256':SHA,'manifestValidated':True}))
