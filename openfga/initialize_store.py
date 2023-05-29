import asyncio
import openfga_sdk
from openfga_sdk.client import OpenFgaClient
from openfga_sdk.api import open_fga_api
from openfga_sdk.models.create_store_request import CreateStoreRequest
from openfga_sdk.client.models.tuple import ClientTuple
from openfga_sdk.client.models.write_request import ClientWriteRequest


def create_store(fga_client_instance):
    try:
        # Create a store
        body = CreateStoreRequest(
            name="IMPERIAL",
        )
        api_response = asyncio.get_event_loop().run_until_complete(
            fga_client_instance.create_store(body))
        return api_response
    except openfga_sdk.ApiException as e:
        print("Exception when calling OpenFgaApi->create_store: %s\n" % e)


def create_authz_model(fga_client_instance):
    model = {
        "schema_version": "1.1",
        "type_definitions": [
            {"type": "user"},
            {
                "type": "resource",
                "relations": {
                        "reader": {
                            "union": {
                                "child": [
                                     {
                                         "this": {}
                                     },
                                    {
                                         "tupleToUserset": {
                                             "tupleset": {
                                                 "relation": "parent"
                                             },
                                             "computedUserset": {
                                                 "relation": "reader"
                                             }
                                         }
                                     }
                                ]
                            }
                        },
                    "writer": {
                            "union": {
                                "child": [
                                    {
                                        "this": {}
                                    },
                                    {
                                        "tupleToUserset": {
                                            "tupleset": {
                                                "relation": "parent"
                                            },
                                            "computedUserset": {
                                                "relation": "writer"
                                            }
                                        }
                                    }
                                ]
                            }
                    },
                    "parent": {
                            "this": {}
                    }
                },
                "metadata": {
                    "relations": {
                        "reader": {
                            "directly_related_user_types": [{"type": "user"}]
                        },
                        "writer": {
                            "directly_related_user_types": [{"type": "user"}]
                        },
                        "parent": {
                            "directly_related_user_types": [{"type": "resource"}]
                        }
                    }
                }
            }
        ]
    }
    api_response = asyncio.get_event_loop().run_until_complete(
        fga_client_instance.write_authorization_model(model))
    return api_response
'''
def _create_write_request(user, relation,object, model_id):
    return ClientWriteRequest(
        writes=TupleKeys(
            tuple_keys=[
                TupleKey(
                    user=user,
                    relation=relation,
                    object=object,
                ),
            ],
        ),
 	    authorization_model_id = model_id,
    )
'''
def create_objects(fga_client_instance):
    '''
    body = _create_write_request("resource:SmartHome-1", "parent", "resource:Address-1", model_id)
    asyncio.get_event_loop().run_until_complete(
        fga_client_instance.write(body))
    body = _create_write_request("resource:SmartHome-1", "parent", "resource:Plug-1", model_id)
    asyncio.get_event_loop().run_until_complete(
        fga_client_instance.write(body))
    body = _create_write_request("resource:Plug-1", "parent", "resource:Power-1", model_id)
    asyncio.get_event_loop().run_until_complete(
        fga_client_instance.write(body))
    body = _create_write_request("resource:SmartHome-1", "parent", "resource:RP-1", model_id)
    asyncio.get_event_loop().run_until_complete(
        fga_client_instance.write(body))
    body = _create_write_request("resource:RP-1", "parent", "resource:Sensor-1", model_id)
    asyncio.get_event_loop().run_until_complete(
        fga_client_instance.write(body))
    body = _create_write_request("resource:Sensor-1", "parent", "resource:Temperature-1", model_id)
    '''

    body = ClientWriteRequest(
        writes=[
            ClientTuple(
                user="resource:SmartHome1",
                relation="parent",
                object="resource:Address1",
            ),
            ClientTuple(
                user="resource:SmartHome1",
                relation="parent",
                object="resource:Plug1",
            ),
            ClientTuple(
                user="resource:Plug1",
                relation="parent",
                object="resource:Power1",
            ),
            ClientTuple(
                user="resource:SmartHome1",
                relation="parent",
                object="resource:RP1",
            ),
            ClientTuple(
                user="resource:RP1",
                relation="parent",
                object="resource:Sensor1",
            ),
            ClientTuple(
                user="resource:Sensor1",
                relation="parent",
                object="resource:Temperature1",
            ),
        ]
    )
    asyncio.get_event_loop().run_until_complete(
        fga_client_instance.write(body))

configuration = openfga_sdk.Configuration(
    api_scheme="http",
    api_host="localhost:8080",
)

# Create an instance of the API class
fga_client_instance = OpenFgaClient(configuration)
new_store = create_store (fga_client_instance)
fga_client_instance.set_store_id(new_store.id)
model = create_authz_model(fga_client_instance)
print(model)
fga_client_instance.set_authorization_model_id(model.authorization_model_id)
create_objects(fga_client_instance)
