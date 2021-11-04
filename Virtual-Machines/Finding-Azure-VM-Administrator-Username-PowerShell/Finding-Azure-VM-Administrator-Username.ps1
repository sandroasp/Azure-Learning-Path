#########################################################
#                                                       #
# Finding Azure VM Administrator Username               #
# Author(s):                                            #
#    Sandro Pereira                                     #
#                                                       #
#########################################################

#use the collection to build up objects for the table
#$connectorDictionary = New-Object "System.Collections.Generic.Dictionary``2[System.String,System.Object]" 

#########################################################
# Perform Azure Login
#########################################################
$azureInfo = az login

Get-AZVM -Name BTS2020LABAVMRG | Select-Object -ExpandProperty OSProfile