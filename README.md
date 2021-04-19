# ByteStreamSerializer-net
a tool to serialize &amp; deserialize custom tcp byte stream based on Attribute and Reflection

<<<<<<< HEAD
A Hello World Example


# 1, define a byte stream
     head                result    
  int - 4bytes         bool - 1byte
# 2, define a class
=======
# applied to Fields 
  APPLY ONLY TO FIELDS **NOT PROPERTIES**



# A Hello World Example


## 1, define a byte stream
        head                result    
         4B                  1B
         int                 bool   
## 2, define a class
>>>>>>> c2dfffb4bb8fafeea93472dadbb3139a1d405a36
~~~
pulic class HelloWorld
{    
      [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int)]
      public int head;


      [ByteStreamParser(FieldType.Field_Bool, FieldType.Field_Bool_Len)]
      public bool result;
}
~~~
  
  
<<<<<<< HEAD
# 3, assembl a  byte stream
=======
## 3, assembl a  byte stream
>>>>>>> c2dfffb4bb8fafeea93472dadbb3139a1d405a36
~~~
PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
IByteBuffer bf = pbba.CompositeBuffer();
bf.WriteInt(21);
bf.WriteBoolean(true);
~~~

<<<<<<< HEAD
# 4, Deserialize
=======
## 4, deserialize
>>>>>>> c2dfffb4bb8fafeea93472dadbb3139a1d405a36
~~~
HelloWorld sdt = ByteStreamToObjectConverter.Deserialize<HelloWorld>(bf);


Assert.AreEqual(21, sdt.head);
Assert.AreEqual(false, sdt.result);
~~~
  
