# ByteStreamSerializer-net
a tool to serialize &amp; deserialize custom tcp byte stream based on Attribute and Reflection

A Hello World Example


# 1, define a byte stream
     head                result    
  int - 4bytes         bool - 1byte
# 2, define a class
~~~
pulic class HelloWorld
{    
      [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int)]
      public int head;


      [ByteStreamParser(FieldType.Field_Bool, FieldType.Field_Bool_Len)]
      public bool result;
}
~~~
  
  
# 3, assembl a  byte stream
~~~
PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
IByteBuffer bf = pbba.CompositeBuffer();
bf.WriteInt(21);
bf.WriteBoolean(true);
~~~

# 4, Deserialize
~~~
HelloWorld sdt = ByteStreamToObjectConverter.Deserialize<HelloWorld>(bf);


Assert.AreEqual(21, sdt.head);
Assert.AreEqual(false, sdt.result);
~~~
  
