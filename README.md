# ByteStreamSerializer-net
a tool to serialize &amp; deserialize custom tcp byte stream based on Attribute and Reflection

# applied to Fields 
  APPLY ONLY TO FIELDS **NOT PROPERTIES**



# A Hello World Example


## 1, define a byte stream
        head                result    
         4B                  1B
         int                 bool   
## 2, define a class
~~~
pulic class HelloWorld
{    
      [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int)]
      public int head;


      [ByteStreamParser(FieldType.Field_Bool, FieldType.Field_Bool_Len)]
      public bool result;
}
~~~
  
  
## 3, assembl a  byte stream
~~~
PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
IByteBuffer bf = pbba.CompositeBuffer();
bf.WriteInt(21);
bf.WriteBoolean(true);
~~~

## 4, deserialize
~~~
HelloWorld sdt = ByteStreamToObjectConverter.Deserialize<HelloWorld>(bf);


Assert.AreEqual(21, sdt.head);
Assert.AreEqual(false, sdt.result);
~~~
  
