# AnalyzerStudio

This is a tool, which allows you to collect information, compare them and helps you make decisions based on them.

## Vocabulary

_Name_ | _Description_
--- | ---
Specimen | A specimen is a concrete object you want to compare against other specimens. Specimen have properties.
Property | Properties have a weight, a data type and a normalization strategy associated with them.
Analysis | Based on your properties, the analysis shows you which specimen best suits your defined weights.
Normalization Strategy | The normalization strategy declares how the values of a property get normalized. "Max" for example maps the values between maximum an 0 to 1 and 0.
Type | The type defines which information is held by a property. The types are: boolean, double (number) and text. Text properties are ignored for the analysis.
Weight | The weight of a property decides how much influence a property value has on the analysis.

## GUI

The GUI consists of three tables: Specimens, Properties and Analysis. All columns can be sorted by clicking on the column header.

### Specimens

The specimens table contains a row for each specimen you added. There is one column for each property you defined and an additional "name" column.
You can edit specimens by double-clicking the row.

### Properties

The properties table has a row for each property you defined. The columns show the name, weight, type and normalization strategy for each property.
You can edit properties by double-clicking the row.

### Analysis

In the analysis table, you can see a row for each specimen you added and a column named "Value".
The value tells you how the values and weights of the properties for each specimen scale against the other specimens.

## Calculation

The property weights and specimen properties are used to calculate the value in the analysis table.
A very important factor is the normalization strategy, which tells the program which value for a property is "better".

After each value was scaled between 1 and 0, the values are summed up and divided by the sum of property weights. This gives a score between 1 and 0.
