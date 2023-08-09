
import pytest
from pdp.openfga_pdp  import OpenFGA_PDP

class TestImperialRelationShips:
    openFGAPDP = OpenFGA_PDP()

    def test_simple_valid(self):
        result = self.openFGAPDP.check("company:ACME", "access","resource:Camera1",[["company:ACME", "access","resource:Camera1"]])
        print(result)
        assert(result.allowed == True)

    def test_advanced_valid(self):
        result = self.openFGAPDP.check("employee:Alice", "access","resource:Camera1",[["company:ACME#authorized", "access","resource:Camera1"],["employee:Alice", "authorized","company:ACME"]])
        print(result)
        assert(result.allowed == True)



    

    
