
import pandas as pd
from pandas import ExcelWriter
from pandas import ExcelFile

import itertools

import json
import math

def process_name_description(fields, dataframe, row_index):
    # field_name = "Name / Description"
    field_name = 1
    field = dataframe.iat[row_index, field_name]
    
    # print("process_name_description field '" + str(field)) + "'"
    
    # name and description are in same cell, they delimited by a period
    name_description = field.split(".", 1)
    
    fields["name"] = name_description[0]
    # print("  Name: " + str(name_description[0]))
    if len(name_description) == 2:
        # print("  Description: " + str(name_description[1]))
        fields["description"] = name_description[1]
    else:
        fields["description"] = ""

spreadsheet_name = 'Draft-DNP3-Profile-for-DER-Communications-2018-06-28.xlsx'


################################################################################
### convert analog inputs tab

df = pd.read_excel(
    spreadsheet_name, 
    sheet_name='AI',
    keep_default_na=False)
 
analog_input_objects = []
analog_input_point_index_map = {}

for row_index in range(4, 379):
    
    if row_index == 340:
        # this cell has point index as AI337 - AI530
        # create separate pointIndex entries for each, create a unique string
        # to match with analog output point indexes
        for i in range(337, 531):
            fields = {}
            fields["pointIndex"] = "AI" + str(i)
            process_name_description(fields, df, row_index)
            fields["defaultEventClass"] = df.iat[row_index, 2]
            fields["minimum"] = df.iat[row_index, 3]
            fields["maximum"] = df.iat[row_index, 4]
            fields["multiplier"] = df.iat[row_index, 5]
            fields["offSet"] = df.iat[row_index, 6]
            fields["units"] = df.iat[row_index, 7]
            fields["resolution"] = df.iat[row_index, 8]
            fields["lnClass"] = df.iat[row_index, 9]
            fields["lninst"] = df.iat[row_index, 10]
            fields["dataObject"] = df.iat[row_index, 11]
            fields["cdc"] = df.iat[row_index, 12]
            fields["reference"] = df.iat[row_index, 13]
            fields["uniqueString"] = df.iat[row_index, 14] + " mapping range " + str(i - 337)
            
            analog_input_objects.append(fields)
            
            # map the name to a point index
            analog_input_point_index_map[fields["uniqueString"]] = fields["pointIndex"]
    else:
      fields = {}
      fields["pointIndex"] = df.iat[row_index, 0]
      process_name_description(fields, df, row_index)
      fields["defaultEventClass"] = df.iat[row_index, 2]
      fields["minimum"] = df.iat[row_index, 3]
      fields["maximum"] = df.iat[row_index, 4]
      fields["multiplier"] = df.iat[row_index, 5]
      fields["offSet"] = df.iat[row_index, 6]
      fields["units"] = df.iat[row_index, 7]
      fields["resolution"] = df.iat[row_index, 8]
      fields["lnClass"] = df.iat[row_index, 9]
      fields["lninst"] = df.iat[row_index, 10]
      fields["dataObject"] = df.iat[row_index, 11]
      fields["cdc"] = df.iat[row_index, 12]
      fields["reference"] = df.iat[row_index, 13]
      fields["uniqueString"] = df.iat[row_index, 14]
      
      # map the name to a point index
      analog_input_point_index_map[fields["uniqueString"]] = fields["pointIndex"]
    
      analog_input_objects.append(fields)

### end convert analog inputs tab
################################################################################


################################################################################
### convert analog outputs tab

df = pd.read_excel(
    spreadsheet_name, 
    sheet_name='AO',
    keep_default_na=False)
 
analog_output_objects = []
analog_output_point_index_map = {}

for row_index in range(4, 271):
    if row_index == 256:
        print("---------------------------------------------------" + str(row_index) + " " + str(df.iat[row_index, 0]))
        # this cell has point index as AO253 - AO446
        # create separate pointIndex entries for each, create a unique string
        # to match with analog input point indexes
        for i in range(253, 447):
            fields = {}
            fields["pointIndex"] = "AO" + str(i)
            process_name_description(fields, df, row_index)
            fields["selectOperate"] = df.iat[row_index, 2]
            fields["directOperate"] = df.iat[row_index, 3]
            fields["directOperateNoAck"] = df.iat[row_index, 4]
            fields["minimum"] = df.iat[row_index, 5]
            fields["maximum"] = df.iat[row_index, 6]
            fields["multiplier"] = df.iat[row_index, 7]
            fields["offSet"] = df.iat[row_index, 8]
            fields["units"] = df.iat[row_index, 9]
            fields["resolution"] = df.iat[row_index, 10]
            fields["chg"] = df.iat[row_index, 11]
            fields["cmd"] = df.iat[row_index, 12]
            fields["lnClass"] = df.iat[row_index, 13]
            fields["lnInst"] = df.iat[row_index, 14]
            fields["dataObject"] = df.iat[row_index, 15]
            fields["cdc"] = df.iat[row_index, 16]
            fields["reference"] = df.iat[row_index, 17]
            # "Unique String" cell in AI tab has a single "." for some reason
            fields["uniqueString"] = df.iat[row_index, 18] + ". mapping range " + str(i - 253)
            
            analog_output_objects.append(fields)
            
            # map the name to a point index
            analog_output_point_index_map[fields["uniqueString"]] = fields["pointIndex"]
    else:
        fields = {}
        fields["pointIndex"] = df.iat[row_index, 0]
        process_name_description(fields, df, row_index)
        fields["selectOperate"] = df.iat[row_index, 2]
        fields["directOperate"] = df.iat[row_index, 3]
        fields["directOperateNoAck"] = df.iat[row_index, 4]
        fields["minimum"] = df.iat[row_index, 5]
        fields["maximum"] = df.iat[row_index, 6]
        fields["multiplier"] = df.iat[row_index, 7]
        fields["offSet"] = df.iat[row_index, 8]
        fields["units"] = df.iat[row_index, 9]
        fields["resolution"] = df.iat[row_index, 10]
        fields["chg"] = df.iat[row_index, 11]
        fields["cmd"] = df.iat[row_index, 12]
        fields["lnClass"] = df.iat[row_index, 13]
        fields["lnInst"] = df.iat[row_index, 14]
        fields["dataObject"] = df.iat[row_index, 15]
        fields["cdc"] = df.iat[row_index, 16]
        fields["reference"] = df.iat[row_index, 17]
        fields["uniqueString"] = df.iat[row_index, 18]
        
        analog_output_objects.append(fields)
        
        # map the name to a point index
        analog_output_point_index_map[fields["uniqueString"]] = fields["pointIndex"]

### end convert analog outputs tab
################################################################################


################################################################################
### convert binary inputs tab

df = pd.read_excel(
    spreadsheet_name, 
    sheet_name='BI',
    keep_default_na=False)

binary_input_objects = []
binary_input_point_index_map = {}

for row_index in range(4, 111):
    fields = {}
    
    fields["pointIndex"] = df.iat[row_index, 0]
    process_name_description(fields, df, row_index)
    fields["defaultEventClass"] = df.iat[row_index, 2]
    fields["state0"] = df.iat[row_index, 3]
    fields["state1"] = df.iat[row_index, 4]
    fields["lnClass"] = df.iat[row_index, 5]
    fields["lnInst"] = df.iat[row_index, 6]
    fields["dataObject"] = df.iat[row_index, 7]
    fields["cdc"] = df.iat[row_index, 8]
    fields["function"] = df.iat[row_index, 9]
    fields["uniqueString"] = df.iat[row_index, 10]
    
    binary_input_objects.append(fields)
    
    # map the name to a point index
    binary_input_point_index_map[fields["uniqueString"]] = fields["pointIndex"]

### end convert binary inputs tab
################################################################################


################################################################################
### convert binary outputs tab

df = pd.read_excel(
    spreadsheet_name, 
    sheet_name='BO',
    keep_default_na=False)

binary_output_objects = []
binary_output_point_index_map = {}

for row_index in range(4, 45):
    fields = {}
    
    fields["pointIndex"] = df.iat[row_index, 0]
    process_name_description(fields, df, row_index)
    fields["selectOperate"] = df.iat[row_index, 2]
    fields["directOperate"] = df.iat[row_index, 3]
    fields["directOperateNoAck"] = df.iat[row_index, 4]
    fields["pulseOn"] = df.iat[row_index, 5]
    fields["pulseOff"] = df.iat[row_index, 6]
    fields["latchOn"] = df.iat[row_index, 7]
    fields["latchOff"] = df.iat[row_index, 8]
    fields["trip"] = df.iat[row_index, 9]
    fields["close"] = df.iat[row_index, 10]
    fields["countGreaterThanOne"] = df.iat[row_index, 11]
    fields["cancelCurrentOperation"] = df.iat[row_index, 12]
    fields["state0"] = df.iat[row_index, 13]
    fields["state1"] = df.iat[row_index, 14]
    fields["defaultCmdClass"] = df.iat[row_index, 15]
    fields["defaultEventClass"] = df.iat[row_index, 16]
    fields["lnClass"] = df.iat[row_index, 17]
    fields["lnInst"] = df.iat[row_index, 18]
    fields["dataObject"] = df.iat[row_index, 19]
    fields["cdc"] = df.iat[row_index, 20]
    fields["uniqueString"] = df.iat[row_index, 21]
    
    binary_output_objects.append(fields)
    
    # map the name to a point index
    binary_output_point_index_map[fields["uniqueString"]] = fields["pointIndex"]

### end convert binary outputs tab
################################################################################


################################################################################
### cretae mapping from input indexs to output indexs

input_to_output_indexes = []

for name in analog_output_point_index_map:
    if name in analog_input_point_index_map:
        input_to_output_indexes.append({
            "aiIndex": analog_input_point_index_map[name],
            "aoIndex": analog_output_point_index_map[name]
        })
    else:
        print("corresponding analog output point index not found for " + analog_output_point_index_map[name] + " '" + name + "'" )

for name in binary_output_point_index_map:
    if name in binary_input_point_index_map:
        input_to_output_indexes.append({
            "biIndex": binary_input_point_index_map[name],
            "boIndex": binary_output_point_index_map[name]
        })
    else:
        print("corresponding binary output point index not found for " + binary_output_point_index_map[name] + " '" + name + "'" )

### end cretae mapping from input indexs to output indexs
################################################################################

# write json file
with open("dnp3_profile_for_der_communications.json", "w") as outputfile:
    data = {
        "binaryInputs": binary_input_objects,
        "binaryOutputs": binary_output_objects,
        "analogInputs": analog_input_objects,
        "analogOutputs": analog_output_objects,
        "map": input_to_output_indexes
    }
    
    outputfile.write(json.dumps(data))
