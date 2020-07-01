package com.hideakin.textsearch.index.service;

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
	public void findText_nogroup() {
		when(fileGroupService.getGid("xyzzy")).thenReturn(-1);
		FindTextResponse rsp = indexService.findText("xyzzy", "FOO", SearchOptions.Exact);
		Assertions.assertEquals(0, rsp.getHits().length);
	}

	@Test
	public void findText_nohit() {
		when(fileGroupService.getGid("xyzzy")).thenReturn(3);
		when(textRepository.findByText("FOO")).thenReturn(null);
		FindTextResponse rsp = indexService.findText("xyzzy", "FOO", SearchOptions.Exact);
		Assertions.assertEquals(0, rsp.getHits().length);
	}

	@Test
	public void findText_exact() {
		when(fileGroupService.getGid("xyzzy")).thenReturn(3);
		TextEntity entity = new TextEntity("FOO", new byte[] { 3, 1, 11, 5, 2, 13, 17, 7, 3, 19, 23, 29 });
		when(textRepository.findByText("FOO")).thenReturn(entity);
		when(fileService.getPath(3)).thenReturn("quux.cpp");
		when(fileService.getPath(5)).thenReturn("fred.cs");
		when(fileService.getPath(7)).thenReturn("waldo.java");
		FindTextResponse rsp = indexService.findText("xyzzy", "FOO", SearchOptions.Exact);
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
		when(fileGroupService.getGid("xyzzy")).thenReturn(4);
		List<TextEntity> entities = new ArrayList<>();
		entities.add(new TextEntity("XBARX", new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }));
		entities.add(new TextEntity("YBARY", new byte[] { 5, 1, 13, 7, 2, 19, 29 }));
		when(textRepository.findAllByTextContaining("BAR")).thenReturn(entities);
		when(fileService.getPath(3)).thenReturn("quux.cpp");
		when(fileService.getPath(5)).thenReturn("fred.cs");
		when(fileService.getPath(7)).thenReturn("waldo.java");
		FindTextResponse rsp = indexService.findText("xyzzy", "BAR", SearchOptions.Contains);
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
		when(fileGroupService.getGid("xyzzy")).thenReturn(4);
		List<TextEntity> entities = new ArrayList<>();
		entities.add(new TextEntity("BARX", new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }));
		entities.add(new TextEntity("BARY", new byte[] { 5, 1, 13, 7, 2, 19, 29 }));
		when(textRepository.findAllByTextStartingWith("BAR")).thenReturn(entities);
		when(fileService.getPath(3)).thenReturn("quux.cpp");
		when(fileService.getPath(5)).thenReturn("fred.cs");
		when(fileService.getPath(7)).thenReturn("waldo.java");
		FindTextResponse rsp = indexService.findText("xyzzy", "BAR", SearchOptions.StartsWith);
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
		when(fileGroupService.getGid("xyzzy")).thenReturn(4);
		List<TextEntity> entities = new ArrayList<>();
		entities.add(new TextEntity("XBAR", new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }));
		entities.add(new TextEntity("YBAR", new byte[] { 5, 1, 13, 7, 2, 19, 29 }));
		when(textRepository.findAllByTextEndingWith("BAR")).thenReturn(entities);
		when(fileService.getPath(3)).thenReturn("quux.cpp");
		when(fileService.getPath(5)).thenReturn("fred.cs");
		when(fileService.getPath(7)).thenReturn("waldo.java");
		FindTextResponse rsp = indexService.findText("xyzzy", "BAR", SearchOptions.EndsWith);
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
