FROM python:3.9

RUN pip3 install Werkzeug
RUN pip3 install jsonpath-ng
RUN pip3 install jwcrypto
RUN pip3 install requests

COPY pep-proxy/ pep-proxy/
ENTRYPOINT [ "python", "pep-proxy/pep-proxy.py" ]
