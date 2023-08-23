import pytest

@pytest.fixture(autouse=True, scope="session")
def PEPProxy():
    import subprocess
    import time
    import os
    my_env = os.environ.copy()
    my_env["PEP_CONF_FILE"] = "../pep/conf/proxy.conf"
    p1 = subprocess.Popen(["python3", '../pep/pep-proxy/pep-proxy.py'],env=my_env)
    time.sleep(5) #Otherwise the server is not ready when tests start
    yield
    p1.terminate()

@pytest.fixture(autouse=True, scope="session")
def HTTPResource():
    import subprocess
    import time
    p1 = subprocess.Popen(['python3', 'tests/http_server.py'])
    time.sleep(5) #Otherwise the server is not ready when tests start
    yield
    p1.terminate()

