package com.hideakin.textsearch.index.service;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyInt;
import static org.mockito.ArgumentMatchers.argThat;
import static org.mockito.Mockito.doNothing;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.util.ArrayList;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.springframework.boot.test.context.SpringBootTest;

import com.hideakin.textsearch.index.entity.FileContentEntity;
import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.model.FileStats;
import com.hideakin.textsearch.index.model.ObjectDisposition;
import com.hideakin.textsearch.index.repository.FileContentRepository;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;
import com.hideakin.textsearch.index.utility.GZipHelper;

@SpringBootTest
public class FileServiceTests {

	@Mock
	private FileRepository fileRepository;

	@Mock
	private FileGroupRepository fileGroupRepository;

	@Mock
	private FileContentRepository fileContentRepository;

	@Mock
	private PreferenceRepository preferenceRepository;

	@InjectMocks
	private FileService fileService = new FileServiceImpl();

	@SuppressWarnings("serial")
	@Test
	public void getFiles_successful() {
		when(fileGroupRepository.findByName("quux")).thenReturn(new FileGroupEntity(333, "quux"));
		when(fileRepository.findAllByGidAndStaleFalse(333)).thenReturn(new ArrayList<FileEntity>() {{
			add(new FileEntity(801, "foo.java", 101, 333));
			add(new FileEntity(802, "bar.java", 202, 333));
			add(new FileEntity(803, "baz.java", 303, 333));
		}});
		FileInfo[] fi = fileService.getFiles("quux");
		Assertions.assertEquals(3, fi.length);
		Assertions.assertEquals(801, fi[0].getFid());
		Assertions.assertEquals("foo.java", fi[0].getPath());
		Assertions.assertEquals(802, fi[1].getFid());
		Assertions.assertEquals("bar.java", fi[1].getPath());
		Assertions.assertEquals(803, fi[2].getFid());
		Assertions.assertEquals("baz.java", fi[2].getPath());
	}

	@Test
	public void getFiles_nogroup() {
		when(fileGroupRepository.findByName("quux")).thenReturn(null);
		FileInfo[] fi = fileService.getFiles("quux");
		Assertions.assertEquals(null, fi);
	}

	@Test
	public void getFiles_null() {
		when(fileGroupRepository.findByName("quux")).thenReturn(new FileGroupEntity(333, "quux"));
		when(fileRepository.findAllByGidAndStaleFalse(333)).thenReturn(new ArrayList<FileEntity>());
		FileInfo[] fi = fileService.getFiles("quux");
		Assertions.assertEquals(0, fi.length);
	}

	@SuppressWarnings("serial")
	@Test
	public void getStats_successful() {
		when(fileGroupRepository.findByName("fred")).thenReturn(new FileGroupEntity(444, "fred"));
		when(fileRepository.findAllByGid(444)).thenReturn(new ArrayList<FileEntity>() {{
			add(new FileEntity(1, "foo.c", 1024, 444));
			add(new FileEntity(2, "bar.c", 2048, 444));
			add(new FileEntity(3, "baz.c", 4096, 444));
		}});
		when(fileContentRepository.findByFid(1)).thenReturn(new FileContentEntity(1, new byte[100]));
		when(fileContentRepository.findByFid(2)).thenReturn(new FileContentEntity(2, new byte[200]));
		when(fileContentRepository.findByFid(3)).thenReturn(new FileContentEntity(3, new byte[300]));
		FileStats stats = fileService.getStats("fred");
		Assertions.assertEquals(444, stats.getGid());
		Assertions.assertEquals("fred", stats.getGroup());
		Assertions.assertEquals(3, stats.getFiles());
		Assertions.assertEquals(7168, stats.getTotalBytes());
		Assertions.assertEquals(600, stats.getTotalStoredBytes());
		Assertions.assertEquals(0, stats.getStaleFiles());
		Assertions.assertEquals(0, stats.getTotalStaleBytes());
		Assertions.assertEquals(0, stats.getTotalStoredStaleBytes());
	}

	@Test
	public void getStats_notFound() {
		when(fileGroupRepository.findByName("fred")).thenReturn(null);
		FileStats stats = fileService.getStats("fred");
		Assertions.assertEquals(null, stats);
	}

	@Test
	public void getStats_empty() {
		when(fileGroupRepository.findByName("fred")).thenReturn(new FileGroupEntity(444, "fred"));
		when(fileRepository.findAllByGid(444)).thenReturn(new ArrayList<FileEntity>());
		FileStats stats = fileService.getStats("fred");
		Assertions.assertEquals(444, stats.getGid());
		Assertions.assertEquals("fred", stats.getGroup());
		Assertions.assertEquals(0, stats.getFiles());
		Assertions.assertEquals(0, stats.getTotalBytes());
		Assertions.assertEquals(0, stats.getTotalStoredBytes());
		Assertions.assertEquals(0, stats.getStaleFiles());
		Assertions.assertEquals(0, stats.getTotalStaleBytes());
		Assertions.assertEquals(0, stats.getTotalStoredStaleBytes());
	}

	@Test
	public void getFile_successful() {
		when(fileRepository.findByFid(11)).thenReturn(new FileEntity(11, "foo.java", 256, 1));
		when(fileGroupRepository.findByGid(1)).thenReturn(new FileGroupEntity(1, "xyzzy"));
		FileInfo info = fileService.getFile(11);
		Assertions.assertEquals(11, info.getFid());
		Assertions.assertEquals("foo.java", info.getPath());
		Assertions.assertEquals(256, info.getSize());
		Assertions.assertEquals(1, info.getGid());
		Assertions.assertEquals("xyzzy", info.getGroup());
	}

	@Test
	public void getFile_notFound() {
		when(fileRepository.findByFid(11)).thenReturn(null);
		FileInfo info = fileService.getFile(11);
		Assertions.assertEquals(null, info);
	}

	@SuppressWarnings("serial")
	@Test
	public void getFile2_successful() {
		when(fileGroupRepository.findByName("xyzzy")).thenReturn(new FileGroupEntity(1, "xyzzy"));
		when(fileRepository.findAllByGidAndPathAndStaleFalse(1, "foo.java")).thenReturn(new ArrayList<FileEntity>() {{
			add(new FileEntity(11, "foo.java", 256, 1));
		}});
		FileInfo info = fileService.getFile("xyzzy", "foo.java");
		Assertions.assertEquals(11, info.getFid());
		Assertions.assertEquals("foo.java", info.getPath());
		Assertions.assertEquals(256, info.getSize());
		Assertions.assertEquals(1, info.getGid());
		Assertions.assertEquals("xyzzy", info.getGroup());
	}

	@Test
	public void getFile2_groupNotFound() {
		when(fileGroupRepository.findByName("xyzzy")).thenReturn(null);
		FileInfo info = fileService.getFile("xyzzy", "foo.java");
		Assertions.assertEquals(null, info);
	}

	@Test
	public void getFile2_pathNotFound() {
		when(fileGroupRepository.findByName("xyzzy")).thenReturn(new FileGroupEntity(1, "xyzzy"));
		when(fileRepository.findAllByGidAndPathAndStaleFalse(1, "foo.java")).thenReturn(new ArrayList<FileEntity>());
		FileInfo info = fileService.getFile("xyzzy", "foo.java");
		Assertions.assertEquals(null, info);
	}

	@Test
	public void getPath_successful() {
		when(fileRepository.findByFid(456)).thenReturn(new FileEntity(456, "/home/plugh/thud.cs", 1000, 789));
		String path = fileService.getPath(456);
		Assertions.assertEquals("/home/plugh/thud.cs", path);
	}

	@Test
	public void getPath_null() {
		when(fileRepository.findByFid(456)).thenReturn(null);
		String path = fileService.getPath(456);
		Assertions.assertEquals(null, path);
	}

	@Test
	public void getContents_successful() {
		when(fileRepository.findByFid(789)).thenReturn(new FileEntity(789, "/home/plugh/waldo.cs", 3, 19));
		when(fileContentRepository.findByFid(789)).thenReturn(new FileContentEntity(789, GZipHelper.compress(new byte[] { 109, 113, 127 })));
		byte[] data = fileService.getContents(789);
		Assertions.assertEquals(3, data.length);
		Assertions.assertEquals(109, data[0]);
		Assertions.assertEquals(113, data[1]);
		Assertions.assertEquals(127, data[2]);
	}

	@Test
	public void getContents_notFound() {
		when(fileRepository.findByFid(789)).thenReturn(null);
		byte[] data = fileService.getContents(789);
		Assertions.assertEquals(null, data);
	}

	@Test
	public void addFile_successful1() {
		when(fileGroupRepository.findByNameForUpdate("xyzzy")).thenReturn(new FileGroupEntity(6, "xyzzy"));
		when(fileRepository.findAllByGidAndPathAndStaleFalse(6, "quux.cs")).thenReturn(new ArrayList<FileEntity>());
		when(preferenceRepository.findByName("FID.next")).thenReturn(new PreferenceEntity("FID.next", "23"));
		when(fileRepository.save(argThat(x -> x != null && x.getFid() == 23))).thenReturn(new FileEntity(23, "quux.cs", 3, 6));
		ObjectDisposition disp = new ObjectDisposition();
		FileInfo info = fileService.addFile("xyzzy", "quux.cs", 3,
				GZipHelper.compress(new byte[] { 101, 103, 107 }),
				disp);
		Assertions.assertEquals(23, info.getFid());
		Assertions.assertEquals("quux.cs", info.getPath());
		Assertions.assertEquals(3, info.getSize());
		Assertions.assertEquals(6, info.getGid());
		Assertions.assertEquals("xyzzy", info.getGroup());
		Assertions.assertEquals(ObjectDisposition.CREATED, disp.getValue());
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals("FID.next") && x.getValue().equals("24")));
		verify(fileRepository, times(1)).save(argThat(x -> x.getFid() == 23));
		verify(fileContentRepository, times(1)).save(argThat(x -> x.getFid() == 23));
		verify(fileGroupRepository, times(1)).save(argThat(x -> x.getGid() == 6));
	}

	@SuppressWarnings("serial")
	@Test
	public void addFile_successful2() {
		when(fileGroupRepository.findByNameForUpdate("xyzzy")).thenReturn(new FileGroupEntity(6, "xyzzy"));
		FileEntity entity = new FileEntity(1, "quux.cs", 3, 6);
		when(fileRepository.findAllByGidAndPathAndStaleFalse(6, "quux.cs")).thenReturn(new ArrayList<FileEntity>() {{ add(entity); }});
		when(fileRepository.save(argThat(x -> x != null && x.getFid() == 1))).thenReturn(entity);
		when(preferenceRepository.findByName("FID.next")).thenReturn(new PreferenceEntity("FID.next", "24"));
		when(fileRepository.save(argThat(x -> x != null && x.getFid() == 24))).thenReturn(new FileEntity(24, "quux.cs", 3, 6));
		ObjectDisposition disp = new ObjectDisposition();
		FileInfo info = fileService.addFile("xyzzy", "quux.cs", 3, GZipHelper.compress(new byte[] { 101, 103, 107 }), disp);
		Assertions.assertEquals(24, info.getFid());
		Assertions.assertEquals("quux.cs", info.getPath());
		Assertions.assertEquals(3, info.getSize());
		Assertions.assertEquals(6, info.getGid());
		Assertions.assertEquals("xyzzy", info.getGroup());
		Assertions.assertEquals(ObjectDisposition.UPDATED, disp.getValue());
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals("FID.next") && x.getValue().equals("25")));
		verify(fileRepository, times(1)).save(argThat(x -> x.getFid() == 1));
		verify(fileRepository, times(1)).save(argThat(x -> x.getFid() == 24));
		verify(fileContentRepository, times(1)).save(argThat(x -> x.getFid() == 24));
		verify(fileGroupRepository, times(1)).save(argThat(x -> x.getGid() == 6));
	}

	@Test
	public void addFile_notFound() {
		when(fileGroupRepository.findByNameForUpdate("xyzzy")).thenReturn(null);
		ObjectDisposition disp = new ObjectDisposition();
		FileInfo info = fileService.addFile("xyzzy", "quux.cs", 3, GZipHelper.compress(new byte[] { 101, 103, 107 }), disp);
		Assertions.assertEquals(null, info);
		Assertions.assertEquals(ObjectDisposition.GROUP_NOT_FOUND, disp.getValue());
	}

	@Test
	public void updateFile_successful() {
		when(fileRepository.findByFid(24)).thenReturn(new FileEntity(24, "quux.cs", 3, 6));
		when(fileGroupRepository.findByGidForUpdate(6)).thenReturn(new FileGroupEntity(6, "xyzzy"));
		when(preferenceRepository.findByName("FID.next")).thenReturn(new PreferenceEntity("FID.next", "25"));
		when(fileRepository.save(argThat(x -> x != null && x.getFid() == 25))).thenReturn(new FileEntity(25, "quux.cs", 3, 6));
		FileInfo info = fileService.updateFile(24, "quux.cs", 3, GZipHelper.compress(new byte[] { 101, 103, 107 }));
		Assertions.assertEquals(25, info.getFid());
		Assertions.assertEquals("quux.cs", info.getPath());
		Assertions.assertEquals(3, info.getSize());
		Assertions.assertEquals(6, info.getGid());
		Assertions.assertEquals("xyzzy", info.getGroup());
		verify(fileRepository, times(1)).save(argThat(x -> x.getFid() == 24));
		verify(fileRepository, times(1)).save(argThat(x -> x.getFid() == 25));
		verify(fileContentRepository, times(1)).save(argThat(x -> x.getFid() == 25));
		verify(fileGroupRepository, times(1)).save(argThat(x -> x.getGid() == 6));
	}

	@Test
	public void updateFile_notFound() {
		when(fileRepository.findByFid(14)).thenReturn(null);
		FileInfo info = fileService.updateFile(14, "quux.cs", 3, GZipHelper.compress(new byte[] { 101, 103, 107 }));
		Assertions.assertEquals(null, info);
	}

	@SuppressWarnings("serial")
	@Test
	public void deleteFiles_successful() {
		when(fileGroupRepository.findByGidForUpdate(111)).thenReturn(new FileGroupEntity(111, "xyzzy"));
		when(fileRepository.findAllByGid(111)).thenReturn(new ArrayList<FileEntity>() {{
			add(new FileEntity(801, "/home/src/quux/foo.java", 100, 111));
			add(new FileEntity(802, "/home/src/quux/bar.java", 200, 111));
			add(new FileEntity(803, "/home/src/quux/baz.java", 300, 111));
		}});
		doNothing().when(fileContentRepository).deleteByFid(801);
		doNothing().when(fileContentRepository).deleteByFid(802);
		doNothing().when(fileContentRepository).deleteByFid(803);
		doNothing().when(fileRepository).deleteByGid(111);
		FileInfo[] fi = fileService.deleteFiles(111);
		Assertions.assertEquals(3, fi.length);
		Assertions.assertEquals(801, fi[0].getFid());
		Assertions.assertEquals(802, fi[1].getFid());
		Assertions.assertEquals(803, fi[2].getFid());
		verify(fileContentRepository, times(1)).deleteByFid(801);
		verify(fileContentRepository, times(1)).deleteByFid(802);
		verify(fileContentRepository, times(1)).deleteByFid(803);
		verify(fileRepository, times(1)).deleteByGid(111);
		verify(fileGroupRepository, times(1)).save(argThat(x -> x.getGid() == 111));
	}

	@Test
	public void deleteFiles_notFound() {
		when(fileGroupRepository.findByGidForUpdate(111)).thenReturn(null);
		FileInfo[] fi = fileService.deleteFiles(111);
		Assertions.assertEquals(0, fi.length);
	}

	@SuppressWarnings("serial")
	@Test
	public void deleteStaleFiles_successful() {
		when(fileGroupRepository.findByGidForUpdate(111)).thenReturn(new FileGroupEntity(111, "xyzzy"));
		when(fileRepository.findAllByGidAndStaleTrue(111)).thenReturn(new ArrayList<FileEntity>() {{
			add(new FileEntity(81, "/home/src/quux/foo.java", 100, 111));
			add(new FileEntity(82, "/home/src/quux/bar.java", 200, 111));
			add(new FileEntity(83, "/home/src/quux/baz.java", 300, 111));
		}});
		doNothing().when(fileContentRepository).deleteByFid(81);
		doNothing().when(fileContentRepository).deleteByFid(82);
		doNothing().when(fileContentRepository).deleteByFid(83);
		doNothing().when(fileRepository).deleteByFid(81);
		doNothing().when(fileRepository).deleteByFid(82);
		doNothing().when(fileRepository).deleteByFid(83);
		FileInfo[] fi = fileService.deleteStaleFiles(111);
		Assertions.assertEquals(3, fi.length);
		Assertions.assertEquals(81, fi[0].getFid());
		Assertions.assertEquals(82, fi[1].getFid());
		Assertions.assertEquals(83, fi[2].getFid());
		verify(fileContentRepository, times(1)).deleteByFid(81);
		verify(fileContentRepository, times(1)).deleteByFid(82);
		verify(fileContentRepository, times(1)).deleteByFid(83);
		verify(fileRepository, times(1)).deleteByFid(81);
		verify(fileRepository, times(1)).deleteByFid(82);
		verify(fileRepository, times(1)).deleteByFid(83);
		verify(fileGroupRepository, times(1)).save(argThat(x -> x.getGid() == 111));
	}

	@Test
	public void deleteFile_successful() {
		when(fileRepository.findByFid(777)).thenReturn(new FileEntity(777, "/home/src/quux/foo.java", 100, 111));
		when(fileGroupRepository.findByGidForUpdate(111)).thenReturn(new FileGroupEntity(111, "xyzzy"));
		doNothing().when(fileContentRepository).deleteByFid(777);
		doNothing().when(fileRepository).deleteByFid(777);
		FileInfo fi = fileService.deleteFile(777);
		Assertions.assertEquals(777, fi.getFid());
		verify(fileContentRepository, times(1)).deleteByFid(777);
		verify(fileRepository, times(1)).deleteByFid(777);
		verify(fileGroupRepository, times(1)).save(argThat(x -> x.getGid() == 111));
	}

	@Test
	public void deleteFile_notFound() {
		when(fileRepository.findByFid(777)).thenReturn(null);
		doNothing().when(fileContentRepository).deleteByFid(anyInt());
		doNothing().when(fileRepository).deleteByFid(anyInt());
		FileInfo fi = fileService.deleteFile(777);
		Assertions.assertEquals(null, fi);
		verify(fileContentRepository, times(0)).deleteByFid(anyInt());
		verify(fileRepository, times(0)).deleteByFid(anyInt());
		verify(fileGroupRepository, times(0)).save(any(FileGroupEntity.class));
	}

	@Test
	public void deleteFile_notFound2() {
		when(fileRepository.findByFid(777)).thenReturn(new FileEntity(777, "/home/src/quux/foo.java", 100, 111));
		when(fileGroupRepository.findByGidForUpdate(111)).thenReturn(null);
		doNothing().when(fileContentRepository).deleteByFid(anyInt());
		doNothing().when(fileRepository).deleteByFid(anyInt());
		FileInfo fi = fileService.deleteFile(777);
		Assertions.assertEquals(null, fi);
		verify(fileContentRepository, times(0)).deleteByFid(anyInt());
		verify(fileRepository, times(0)).deleteByFid(anyInt());
		verify(fileGroupRepository, times(0)).save(any(FileGroupEntity.class));
	}

}
