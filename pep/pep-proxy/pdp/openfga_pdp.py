import asyncio
import openfga_sdk
from openfga_sdk.api import open_fga_api
from openfga_sdk.models.check_request import CheckRequest
from openfga_sdk.models.tuple_key import TupleKey
from openfga_sdk.models.contextual_tuple_keys import ContextualTupleKeys

class OpenFGA_PDP:
    def __init__(self):
        configuration = openfga_sdk.Configuration(
            api_scheme="http",
            api_host="localhost:8080",
        )
        self.fga_client = open_fga_api.OpenFgaApi(
        openfga_sdk.ApiClient(configuration))
        response = asyncio.get_event_loop().run_until_complete(
            self.fga_client.list_stores())
        self.fga_client.api_client.set_store_id(response.stores[0].id)
        response = asyncio.get_event_loop().run_until_complete(
            self.fga_client.read_authorization_models())
        self.model_id = response.authorization_models[0].id
        
    def check(self, user, resource, relations):
        tuple_keys = []
        user = user.split(":")[2].strip()
        for items in relations:
            for object,relation in items.items():
                tuple_keys.append(TupleKey(user="user:"+user, relation=relation, object="resource:" + object))
        body = CheckRequest(
                tuple_key=TupleKey(
                    user="user:"+user,
                    relation="reader",
                    object="resource:" + resource,
                ), 
                contextual_tuples=ContextualTupleKeys(
                    tuple_keys=tuple_keys,
                ),
                authorization_model_id=self.model_id,
            )
        response = asyncio.get_event_loop().run_until_complete(
            self.fga_client.check(body))
        return response

if __name__ == '__main__':
    openFGAPDP = OpenFGA_PDP()
    result = openFGAPDP.check([{"user":"user:Nikos", "relation":"reader", "object":"resource:SmartHome1"}])
    print(result.allowed)