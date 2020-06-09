package com.hideakin.textsearch.index.service;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.EntityManager;

import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.springframework.boot.test.context.SpringBootTest;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.model.FindTextResponse;
import com.hideakin.textsearch.index.model.UpdateIndexRequest;
import com.hideakin.textsearch.index.model.UpdateIndexResponse;
import com.hideakin.textsearch.index.repository.TextRepository;

@SpringBootTest
public class IndexServiceTests {

	@Mock
	private EntityManager em;

	@Mock
	private FileGroupService fileGroupService;

	@Mock
	private FileService fileService;

	@Mock
	private TextRepository textRepository;

	@InjectMocks
	private IndexService indexService = new IndexServiceImpl();

	@Test
	public void updateIndex_successful_first() {
		String group = "xyzzy";
		int gid = 1;
		String path = "quux.h";
		int fid = 19;
		List<String> texts = new ArrayList<String>();
		texts.add("FOO");
		texts.add("BAR");
		texts.add("BAZ");
		when(fileGroupService.getGid(group)).thenReturn(-1);
		when(fileGroupService.addGroup(group)).thenReturn(gid);
		when(fileService.getFid(path, gid)).thenReturn(-1);
		when(fileService.addFile(path, gid)).thenReturn(fid);
		PseudoQuery q = new PseudoQuery(new ArrayList<String>());
		when(em.createQuery("SELECT text FROM texts")).thenReturn(q);
		when(textRepository.findByText(texts.get(0))).thenReturn(null);
		when(textRepository.findByText(texts.get(1))).thenReturn(null);
		when(textRepository.findByText(texts.get(2))).thenReturn(null);
		when(textRepository.save(any(TextEntity.class))).thenReturn(null);
		UpdateIndexRequest req = new UpdateIndexRequest();
		req.setPath(path);
		req.setTexts(texts.toArray(new String[texts.size()]));
		UpdateIndexResponse rsp = indexService.updateIndex(group, req);
		Assertions.assertEquals(path, rsp.getPath());
		Assertions.assertEquals(3, rsp.getTexts().length);
	}

	@Test
	public void updateIndex_successful_second() {
		String group = "xyzzy";
		int gid = 2;
		String path = "fred.h";
		int fid = 11;
		List<String> texts = new ArrayList<String>();
		texts.add("FOO");
		texts.add("BAR");
		texts.add("BAZ");
		when(fileGroupService.getGid(group)).thenReturn(-1);
		when(fileGroupService.addGroup(group)).thenReturn(gid);
		when(fileService.getFid(path, gid)).thenReturn(-1);
		when(fileService.addFile(path, gid)).thenReturn(fid);
		PseudoQuery q = new PseudoQuery(texts);
		when(em.createQuery("SELECT text FROM texts")).thenReturn(q);
		TextEntity entity;
		entity = new TextEntity(texts.get(0), new byte[] { 13, 3, 0, 3, 5 });
		when(textRepository.findByText(texts.get(0))).thenReturn(entity);
		entity = new TextEntity(texts.get(1), new byte[] { 13, 2, 1, 4 });
		when(textRepository.findByText(texts.get(1))).thenReturn(entity);
		entity = new TextEntity(texts.get(2), new byte[] { 13, 1, 2 });
		when(textRepository.findByText(texts.get(2))).thenReturn(entity);
		when(textRepository.save(any(TextEntity.class))).thenReturn(null);
		UpdateIndexRequest req = new UpdateIndexRequest();
		req.setPath(path);
		req.setTexts(texts.toArray(new String[texts.size()]));
		UpdateIndexResponse rsp = indexService.updateIndex(group, req);
		Assertions.assertEquals(path, rsp.getPath());
		Assertions.assertEquals(3, rsp.getTexts().length);
		verify(textRepository, times(6)).save(any(TextEntity.class));
		verify(textRepository, times(0)).delete(any(TextEntity.class));
	}

	@Test
	public void updateIndex_successful_overwrite() {
		String group = "xyzzy";
		int gid = 3;
		String path = "waldo.h";
		int fid = 17;
		List<String> texts = new ArrayList<String>();
		texts.add("FOO");
		texts.add("BAR");
		texts.add("BAZ");
		when(fileGroupService.getGid(group)).thenReturn(gid);
		when(fileService.getFid(path, gid)).thenReturn(fid);
		PseudoQuery q = new PseudoQuery(texts);
		when(em.createQuery("SELECT text FROM texts")).thenReturn(q);
		TextEntity entity;
		entity = new TextEntity(texts.get(0), new byte[] { 17, 3, 0, 3, 5 });
		when(textRepository.findByText(texts.get(0))).thenReturn(entity);
		entity = new TextEntity(texts.get(1), new byte[] { 17, 2, 1, 4 });
		when(textRepository.findByText(texts.get(1))).thenReturn(entity);
		entity = new TextEntity(texts.get(2), new byte[] { 17, 1, 2 });
		when(textRepository.findByText(texts.get(2))).thenReturn(entity);
		when(textRepository.save(any(TextEntity.class))).thenReturn(null);
		UpdateIndexRequest req = new UpdateIndexRequest(path, texts.toArray(new String[texts.size()]));
		UpdateIndexResponse rsp = indexService.updateIndex(group, req);
		Assertions.assertEquals(path, rsp.getPath());
		Assertions.assertEquals(3, rsp.getTexts().length);
		verify(textRepository, times(3)).save(any(TextEntity.class));
		verify(textRepository, times(3)).delete(any(TextEntity.class));
	}

	@Test
	public void updateIndex_successful_merge() {
		String group = "xyzzy";
		int gid = 3;
		String path = "thud.h";
		int fid = 17;
		List<String> texts = new ArrayList<String>();
		texts.add("FOO");
		texts.add("BAR");
		texts.add("BAZ");
		when(fileGroupService.getGid(group)).thenReturn(gid);
		when(fileService.getFid(path, gid)).thenReturn(fid);
		PseudoQuery q = new PseudoQuery(texts);
		when(em.createQuery("SELECT text FROM texts")).thenReturn(q);
		TextEntity entity;
		entity = new TextEntity(texts.get(0), new byte[] { 16, 1, 0, 17, 3, 0, 3, 5 });
		when(textRepository.findByText(texts.get(0))).thenReturn(entity);
		entity = new TextEntity(texts.get(1), new byte[] { 17, 2, 1, 4, 19, 2, 0, 1 });
		when(textRepository.findByText(texts.get(1))).thenReturn(entity);
		entity = new TextEntity(texts.get(2), new byte[] { 7, 1, 0, 17, 1, 2, 23, 1, 0 });
		when(textRepository.findByText(texts.get(2))).thenReturn(entity);
		when(textRepository.save(any(TextEntity.class))).thenReturn(null);
		UpdateIndexRequest req = new UpdateIndexRequest(path, texts.toArray(new String[texts.size()]));
		UpdateIndexResponse rsp = indexService.updateIndex(group, req);
		Assertions.assertEquals(path, rsp.getPath());
		Assertions.assertEquals(3, rsp.getTexts().length);
		verify(textRepository, times(6)).save(any(TextEntity.class));
		verify(textRepository, times(0)).delete(any(TextEntity.class));
	}

	@Test
	public void deleteIndex_successful() {
		String group = "quux";
		int gid = 7;
		when(fileGroupService.getGid(group)).thenReturn(gid);
		PseudoQuery q = new PseudoQuery(new ArrayList<String>());
		when(em.createQuery("SELECT text FROM texts")).thenReturn(q);
		List<Integer> fids = new ArrayList<Integer>();
		fids.add(11);
		fids.add(12);
		fids.add(13);
		when(fileService.getFids(gid)).thenReturn(fids);
		indexService.deleteIndex(group);
		verify(fileService, times(1)).getFids(gid);
		verify(fileService, times(1)).delete(fids);
		verify(fileGroupService, times(1)).delete(gid);
	}

	@Test
	public void deleteIndex_nogroup() {
		String group = "quux";
		when(fileGroupService.getGid(group)).thenReturn(-1);
		indexService.deleteIndex(group);
		verify(fileService, times(0)).getFids(any(int.class));
		verify(fileGroupService, times(0)).delete(any(int.class));
	}

	@Test
	public void findText_nogroup() {
		String group = "xyzzy";
		when(fileGroupService.getGid(group)).thenReturn(-1);
		FindTextResponse rsp = indexService.findText(group, "FOO", SearchOptions.Exact);
		Assertions.assertEquals(0, rsp.getHits().length);
	}

	@Test
	public void findText_nohit() {
		String group = "xyzzy";
		String text = "FOO";
		int gid = 3;
		when(fileGroupService.getGid(group)).thenReturn(gid);
		when(textRepository.findByText(text)).thenReturn(null);
		FindTextResponse rsp = indexService.findText(group, text, SearchOptions.Exact);
		Assertions.assertEquals(0, rsp.getHits().length);
	}

	@Test
	public void findText_exact() {
		String group = "xyzzy";
		String text = "FOO";
		int gid = 3;
		when(fileGroupService.getGid(group)).thenReturn(gid);
		TextEntity entity = new TextEntity(text, new byte[] { 3, 1, 11, 5, 2, 13, 17, 7, 3, 19, 23, 29 });
		when(textRepository.findByText(text)).thenReturn(entity);
		when(fileService.getPath(3, gid)).thenReturn("quux.cpp");
		when(fileService.getPath(5, gid)).thenReturn("fred.cs");
		when(fileService.getPath(7, gid)).thenReturn("waldo.java");
		FindTextResponse rsp = indexService.findText(group, text, SearchOptions.Exact);
		Assertions.assertEquals(3, rsp.getHits().length);
		Assertions.assertEquals("quux.cpp", rsp.getHits()[0].getPath());
		Assertions.assertEquals(1, rsp.getHits()[0].getPositions().length);
		Assertions.assertEquals(11, rsp.getHits()[0].getPositions()[0]);
		Assertions.assertEquals("fred.cs", rsp.getHits()[1].getPath());
		Assertions.assertEquals(2, rsp.getHits()[1].getPositions().length);
		Assertions.assertEquals(13, rsp.getHits()[1].getPositions()[0]);
		Assertions.assertEquals(17, rsp.getHits()[1].getPositions()[1]);
		Assertions.assertEquals("waldo.java", rsp.getHits()[2].getPath());
		Assertions.assertEquals(3, rsp.getHits()[2].getPositions().length);
		Assertions.assertEquals(19, rsp.getHits()[2].getPositions()[0]);
		Assertions.assertEquals(23, rsp.getHits()[2].getPositions()[1]);
		Assertions.assertEquals(29, rsp.getHits()[2].getPositions()[2]);
	}

	@Test
	public void findText_contains() {
		String group = "xyzzy";
		String text = "BAR";
		int gid = 4;
		when(fileGroupService.getGid(group)).thenReturn(gid);
		List<TextEntity> entities = new ArrayList<>();
		entities.add(new TextEntity(text, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }));
		entities.add(new TextEntity(text, new byte[] { 5, 1, 13, 7, 2, 19, 29 }));
		when(textRepository.findAllByTextContaining(text)).thenReturn(entities);
		when(fileService.getPath(3, gid)).thenReturn("quux.cpp");
		when(fileService.getPath(5, gid)).thenReturn("fred.cs");
		when(fileService.getPath(7, gid)).thenReturn("waldo.java");
		FindTextResponse rsp = indexService.findText(group, text, SearchOptions.Contains);
		Assertions.assertEquals(3, rsp.getHits().length);
		Assertions.assertEquals("quux.cpp", rsp.getHits()[0].getPath());
		Assertions.assertEquals(1, rsp.getHits()[0].getPositions().length);
		Assertions.assertEquals(11, rsp.getHits()[0].getPositions()[0]);
		Assertions.assertEquals("fred.cs", rsp.getHits()[1].getPath());
		Assertions.assertEquals(2, rsp.getHits()[1].getPositions().length);
		Assertions.assertEquals(13, rsp.getHits()[1].getPositions()[0]);
		Assertions.assertEquals(17, rsp.getHits()[1].getPositions()[1]);
		Assertions.assertEquals("waldo.java", rsp.getHits()[2].getPath());
		Assertions.assertEquals(3, rsp.getHits()[2].getPositions().length);
		Assertions.assertEquals(19, rsp.getHits()[2].getPositions()[0]);
		Assertions.assertEquals(23, rsp.getHits()[2].getPositions()[1]);
		Assertions.assertEquals(29, rsp.getHits()[2].getPositions()[2]);
	}

	@Test
	public void findText_startsWith() {
		String group = "xyzzy";
		String text = "BAR";
		int gid = 4;
		when(fileGroupService.getGid(group)).thenReturn(gid);
		List<TextEntity> entities = new ArrayList<>();
		entities.add(new TextEntity(text, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }));
		entities.add(new TextEntity(text, new byte[] { 5, 1, 13, 7, 2, 19, 29 }));
		when(textRepository.findAllByTextStartingWith(text)).thenReturn(entities);
		when(fileService.getPath(3, gid)).thenReturn("quux.cpp");
		when(fileService.getPath(5, gid)).thenReturn("fred.cs");
		when(fileService.getPath(7, gid)).thenReturn("waldo.java");
		FindTextResponse rsp = indexService.findText(group, text, SearchOptions.StartsWith);
		Assertions.assertEquals(3, rsp.getHits().length);
		Assertions.assertEquals("quux.cpp", rsp.getHits()[0].getPath());
		Assertions.assertEquals(1, rsp.getHits()[0].getPositions().length);
		Assertions.assertEquals(11, rsp.getHits()[0].getPositions()[0]);
		Assertions.assertEquals("fred.cs", rsp.getHits()[1].getPath());
		Assertions.assertEquals(2, rsp.getHits()[1].getPositions().length);
		Assertions.assertEquals(13, rsp.getHits()[1].getPositions()[0]);
		Assertions.assertEquals(17, rsp.getHits()[1].getPositions()[1]);
		Assertions.assertEquals("waldo.java", rsp.getHits()[2].getPath());
		Assertions.assertEquals(3, rsp.getHits()[2].getPositions().length);
		Assertions.assertEquals(19, rsp.getHits()[2].getPositions()[0]);
		Assertions.assertEquals(23, rsp.getHits()[2].getPositions()[1]);
		Assertions.assertEquals(29, rsp.getHits()[2].getPositions()[2]);
	}

	@Test
	public void findText_endsWith() {
		String group = "xyzzy";
		String text = "BAR";
		int gid = 4;
		when(fileGroupService.getGid(group)).thenReturn(gid);
		List<TextEntity> entities = new ArrayList<>();
		entities.add(new TextEntity(text, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }));
		entities.add(new TextEntity(text, new byte[] { 5, 1, 13, 7, 2, 19, 29 }));
		when(textRepository.findAllByTextEndingWith(text)).thenReturn(entities);
		when(fileService.getPath(3, gid)).thenReturn("quux.cpp");
		when(fileService.getPath(5, gid)).thenReturn("fred.cs");
		when(fileService.getPath(7, gid)).thenReturn("waldo.java");
		FindTextResponse rsp = indexService.findText(group, text, SearchOptions.EndsWith);
		Assertions.assertEquals(3, rsp.getHits().length);
		Assertions.assertEquals("quux.cpp", rsp.getHits()[0].getPath());
		Assertions.assertEquals(1, rsp.getHits()[0].getPositions().length);
		Assertions.assertEquals(11, rsp.getHits()[0].getPositions()[0]);
		Assertions.assertEquals("fred.cs", rsp.getHits()[1].getPath());
		Assertions.assertEquals(2, rsp.getHits()[1].getPositions().length);
		Assertions.assertEquals(13, rsp.getHits()[1].getPositions()[0]);
		Assertions.assertEquals(17, rsp.getHits()[1].getPositions()[1]);
		Assertions.assertEquals("waldo.java", rsp.getHits()[2].getPath());
		Assertions.assertEquals(3, rsp.getHits()[2].getPositions().length);
		Assertions.assertEquals(19, rsp.getHits()[2].getPositions()[0]);
		Assertions.assertEquals(23, rsp.getHits()[2].getPositions()[1]);
		Assertions.assertEquals(29, rsp.getHits()[2].getPositions()[2]);
	}

}
