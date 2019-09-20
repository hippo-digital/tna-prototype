# tna-prototype

## Prerequisites 

Download the stanford core NLP models from [here](http://nlp.stanford.edu/software/stanford-corenlp-full-2018-10-05.zip)
also download the KBP dictionaries from [here](http://nlp.stanford.edu/software/stanford-english-kbp-corenlp-2018-10-05-models.jar)

The NLP models zips needs be extracted in to the root of the KnowledgeGraphBuilder. The stanford-corenlp-3.7.0-models.jar then should be unzipped into this folder, once this is complete the KBP download needs to be extract then needs to be then copied into the extracted models folder, the structure is already aligned so this just involves copying the `edu` folder onto the models `edu` folder.

## Running NEO4J

The solution assumes NEO4J is running in a docker container on the local machine. To create a neo4j docker instance run 

    docker pull neo4j 
    
    docker run --publish=7474:7474 --publish=7687:7687 --volume=$HOME/neo4j/data:/data neo4j
    
Once the container is up and running you should be able to navigate to `localhost:7474` and see the Neo4J admin console. 

## Running the application

Open the solution file in Visual Studio / JetBrains Rider and run the `GraphBuilder` project. This will build a graph based on the `C14242` series. 

Once the load has finished you can now navigate to the Neo4j console and run a query to see a graph for example, 

    MATCH p=(n:location)-[:title|:nationality|:city|:person|:documenttype|:date*0..2]-(a) WHERE n.name = "Crimea" or n.name="Hospital" RETURN DISTINCT n, collect(a)[..25]
    
should return something like 

    ![example/Crimea_Hospitals_Graph.png](Crimea Hospital Graph)
    
**Optional**

You can also run the `FeatureExtractor` project. This will output a set of JSON files to the bin directory of this project. These can then be copied to the `GraphBuilder\Data` folder and the `GraphBuilder` project will then load all of the results to the Neo4J instance. As a word of warning the FeatureExtractor can be quiet slow as it is a naive implementation currently. 