## Testing

### Prerequisites
Tests are executed using pytest and pytest-asyncio. To install it execute: 

```bash
python3 -m pip install  pytest 
python3 -m pip install pytest-asyncio
```

### Running the tests

Execute OpenFGA docker image using the following command:

```bash
docker run -d --rm -p 5004:8080 -p 5005:8081 -p 5006:3000 openfga/openfga run
```

Then execute the `initialize_store.py` located inside the openfga directory. 
Finally, from the root directory run `python3 -m pytest -s  tests/` For shorter output alternatively you can run `python3 -m pytest tests/ -s --tb=short`